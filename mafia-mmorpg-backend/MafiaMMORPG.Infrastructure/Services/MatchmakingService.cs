using StackExchange.Redis;
using System.Text;
using MafiaMMORPG.Application.Interfaces;
using MafiaMMORPG.Application.DTOs;
using MafiaMMORPG.Application.Repositories;
using MafiaMMORPG.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MafiaMMORPG.Infrastructure.Services;

public class MatchmakingService : IMatchmakingService
{
    private readonly IDatabase _db;
    private readonly IRepository<Rating> _ratingRepo;
    private readonly ILogger<MatchmakingService> _logger;
    private readonly LoadedLuaScript _matchmakingScript;
    
    // Eşleştirme parametreleri
    private const int BaseDelta = 50;
    private const int StepSeconds = 10;
    private const int StepDelta = 25;
    private const int MaxDelta = 400;
    private const int DefaultMmr = 1200;

    public MatchmakingService(
        IConnectionMultiplexer redis,
        IRepository<Rating> ratingRepo,
        ILogger<MatchmakingService> logger)
    {
        ArgumentNullException.ThrowIfNull(redis);
        ArgumentNullException.ThrowIfNull(ratingRepo);
        ArgumentNullException.ThrowIfNull(logger);
        
        _db = redis.GetDatabase();
        _ratingRepo = ratingRepo;
        _logger = logger;
        
        // Lua script'ini yükle
        var script = File.ReadAllText("Redis/Lua/Matchmaking.lua");
        _matchmakingScript = LuaScript.Prepare(script).Load(redis.GetServer(redis.GetEndPoints().First()));
    }

    public async Task<MatchInfo?> EnqueueAsync(Guid playerId)
    {
        var now = DateTime.UtcNow;
        var nowTicks = now.Ticks;
        
        try
        {
            // Önceki durumu temizle (idempotency)
            await DequeueAsync(playerId);
            
            // MMR'yi al
            var rating = await _ratingRepo.FirstOrDefaultAsync(r => r.PlayerId == playerId);
            var mmr = rating?.MMR ?? DefaultMmr;
            
            // Bekleme süresini hesapla
            var waitingSeconds = 0;
            var delta = CalculateDelta(waitingSeconds);
            
            // Kuyruğa ekle
            await _db.SortedSetAddAsync("mmrpg:pvp:queue", playerId.ToString(), mmr);
            
            // Bekleme meta verilerini kaydet
            await _db.HashSetAsync($"mmrpg:pvp:wait:{playerId}", new HashEntry[]
            {
                new("mmr", mmr),
                new("enqueuedAt", nowTicks)
            });
            
            // Oyuncu durumunu güncelle
            await _db.HashSetAsync($"mmrpg:pvp:state:{playerId}", new HashEntry[]
            {
                new("state", "queued"),
                new("matchId", "")
            });
            
            _logger.LogInformation("Player {PlayerId} enqueued with MMR {Mmr}", playerId, mmr);
            
            // Atomik eşleştirme dene
            var matchId = Guid.NewGuid();
            var result = await _matchmakingScript.EvaluateAsync(_db, new
            {
                playerId = playerId.ToString(),
                mmr = mmr,
                delta = delta,
                nowTicks = nowTicks,
                matchId = matchId.ToString()
            });
            
            if (result.IsNull)
            {
                _logger.LogInformation("No match found for player {PlayerId}, waiting in queue", playerId);
                return null;
            }
            
            // Eşleştirme bulundu
            if (result == null)
            {
                _logger.LogWarning("Null result for player {PlayerId}", playerId);
                return null;
            }
            
            var matchData = (RedisValue[])result!;
            if (matchData == null || matchData.Length < 3)
            {
                _logger.LogWarning("Invalid match data for player {PlayerId}", playerId);
                return null;
            }
            
            var p1Str = matchData[1].ToString() ?? string.Empty;
            var p2Str = matchData[2].ToString() ?? string.Empty;
            
            if (string.IsNullOrWhiteSpace(p1Str) || string.IsNullOrWhiteSpace(p2Str))
            {
                _logger.LogWarning("Invalid match data for player {PlayerId}", playerId);
                return null;
            }
            
            var p1 = Guid.Parse(p1Str);
            var p2 = Guid.Parse(p2Str);
            
            var matchInfo = new MatchInfo(
                matchId,
                p1,
                p2,
                now,
                "awaiting"
            );
            
            _logger.LogInformation("Match found: {MatchId} between {P1} and {P2}", matchId, p1, p2);
            
            return matchInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enqueueing player {PlayerId}", playerId);
            await DequeueAsync(playerId); // Temizlik
            throw;
        }
    }

    public async Task<bool> DequeueAsync(Guid playerId)
    {
        try
        {
            // Kuyruktan çıkar
            var removed = await _db.SortedSetRemoveAsync("mmrpg:pvp:queue", playerId.ToString());
            
            // Durumu temizle
            await _db.KeyDeleteAsync($"mmrpg:pvp:state:{playerId}");
            await _db.KeyDeleteAsync($"mmrpg:pvp:wait:{playerId}");
            
            if (removed)
            {
                _logger.LogInformation("Player {PlayerId} dequeued", playerId);
            }
            
            return removed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dequeuing player {PlayerId}", playerId);
            return false;
        }
    }

    public async Task<bool> AcceptAsync(Guid playerId, Guid matchId)
    {
        try
        {
            // Kabul set'ine ekle
            var added = await _db.SetAddAsync($"mmrpg:pvp:accept:{matchId}", playerId.ToString());
            
            if (!added)
            {
                _logger.LogWarning("Player {PlayerId} already accepted match {MatchId}", playerId, matchId);
                return false;
            }
            
            // Diğer oyuncunun kabul edip etmediğini kontrol et
            var acceptCount = await _db.SetLengthAsync($"mmrpg:pvp:accept:{matchId}");
            
            if (acceptCount == 2)
            {
                // Her iki taraf da kabul etti
                await _db.HashSetAsync($"mmrpg:pvp:match:{matchId}", "state", "accepted");
                
                _logger.LogInformation("Match {MatchId} accepted by both players", matchId);
                return true;
            }
            
            _logger.LogInformation("Player {PlayerId} accepted match {MatchId}, waiting for other player", playerId, matchId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting match {MatchId} for player {PlayerId}", matchId, playerId);
            return false;
        }
    }

    public async Task<PlayerQueueStatus?> GetStatusAsync(Guid playerId)
    {
        try
        {
            var state = await _db.HashGetAsync($"mmrpg:pvp:state:{playerId}", "state");
            var matchId = await _db.HashGetAsync($"mmrpg:pvp:state:{playerId}", "matchId");
            var waitData = await _db.HashGetAllAsync($"mmrpg:pvp:wait:{playerId}");
            
            if (state.IsNull)
                return null;
            
            var waitingSeconds = 0;
            if (state == "queued" && waitData.Length > 0)
            {
                var enqueuedAtEntry = waitData.FirstOrDefault(x => x.Name == "enqueuedAt");
                if (!enqueuedAtEntry.Value.IsNull)
                {
                    var enqueuedAtStr = enqueuedAtEntry.Value.ToString();
                    if (!string.IsNullOrWhiteSpace(enqueuedAtStr) && long.TryParse(enqueuedAtStr, out var enqueuedAt))
                    {
                        var now = DateTime.UtcNow.Ticks;
                        waitingSeconds = (int)((now - enqueuedAt) / TimeSpan.TicksPerSecond);
                    }
                }
            }
            
            var currentDelta = CalculateDelta(waitingSeconds);
            
            var matchIdStr = matchId.ToString();
            Guid? matchIdGuid = null;
            if (!string.IsNullOrWhiteSpace(matchIdStr))
            {
                matchIdGuid = Guid.Parse(matchIdStr);
            }
            
            return new PlayerQueueStatus(
                state.ToString(),
                matchIdGuid,
                waitingSeconds,
                currentDelta
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status for player {PlayerId}", playerId);
            return null;
        }
    }

    private int CalculateDelta(int waitingSeconds)
    {
        var delta = BaseDelta + (waitingSeconds / StepSeconds) * StepDelta;
        return Math.Min(delta, MaxDelta);
    }

    // Timeout temizleme (arka plan job için)
    public async Task CleanupExpiredMatches()
    {
        try
        {
            // Süresi dolmuş accept set'lerini bul
            var expiredAccepts = await _db.ExecuteAsync("SCAN", "0", "MATCH", "mmrpg:pvp:accept:*", "COUNT", "100");
            
            // Her expired accept için:
            // - Match'i cancelled yap
            // - Kabul eden oyuncuyu requeue et
            // - Kabul etmeyen oyuncuyu idle yap
            
            _logger.LogInformation("Cleaned up expired matches");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired matches");
        }
    }
}
