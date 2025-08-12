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
    private readonly IConnectionMultiplexer _redis;
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
        _redis = redis;
        _ratingRepo = ratingRepo;
        _logger = logger;
        
        // Lua script'ini yükle
        var script = File.ReadAllText("Redis/Lua/Matchmaking.lua");
        _matchmakingScript = LuaScript.Prepare(script).Load(redis.GetServer(redis.GetEndPoints().First()));
    }

    public async Task<MatchInfo?> EnqueueAsync(Guid playerId)
    {
        var db = _redis.GetDatabase();
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
            await db.SortedSetAddAsync("mmrpg:pvp:queue", playerId.ToString(), mmr);
            
            // Bekleme meta verilerini kaydet
            await db.HashSetAsync($"mmrpg:pvp:wait:{playerId}", new HashEntry[]
            {
                new("mmr", mmr),
                new("enqueuedAt", nowTicks)
            });
            
            // Oyuncu durumunu güncelle
            await db.HashSetAsync($"mmrpg:pvp:state:{playerId}", new HashEntry[]
            {
                new("state", "queued"),
                new("matchId", "")
            });
            
            _logger.LogInformation("Player {PlayerId} enqueued with MMR {Mmr}", playerId, mmr);
            
            // Atomik eşleştirme dene
            var matchId = Guid.NewGuid();
            var result = await _matchmakingScript.EvaluateAsync(db, new
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
            var matchData = (RedisValue[])result;
            var p1 = Guid.Parse(matchData[1]);
            var p2 = Guid.Parse(matchData[2]);
            
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
        var db = _redis.GetDatabase();
        
        try
        {
            // Kuyruktan çıkar
            var removed = await db.SortedSetRemoveAsync("mmrpg:pvp:queue", playerId.ToString());
            
            // Durumu temizle
            await db.KeyDeleteAsync($"mmrpg:pvp:state:{playerId}");
            await db.KeyDeleteAsync($"mmrpg:pvp:wait:{playerId}");
            
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
        var db = _redis.GetDatabase();
        
        try
        {
            // Kabul set'ine ekle
            var added = await db.SetAddAsync($"mmrpg:pvp:accept:{matchId}", playerId.ToString());
            
            if (!added)
            {
                _logger.LogWarning("Player {PlayerId} already accepted match {MatchId}", playerId, matchId);
                return false;
            }
            
            // Diğer oyuncunun kabul edip etmediğini kontrol et
            var acceptCount = await db.SetLengthAsync($"mmrpg:pvp:accept:{matchId}");
            
            if (acceptCount == 2)
            {
                // Her iki taraf da kabul etti
                await db.HashSetAsync($"mmrpg:pvp:match:{matchId}", "state", "accepted");
                
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
        var db = _redis.GetDatabase();
        
        try
        {
            var state = await db.HashGetAsync($"mmrpg:pvp:state:{playerId}", "state");
            var matchId = await db.HashGetAsync($"mmrpg:pvp:state:{playerId}", "matchId");
            var waitData = await db.HashGetAllAsync($"mmrpg:pvp:wait:{playerId}");
            
            if (state.IsNull)
                return null;
            
            var waitingSeconds = 0;
            if (state == "queued" && waitData.Length > 0)
            {
                var enqueuedAt = long.Parse(waitData.FirstOrDefault(x => x.Name == "enqueuedAt").Value);
                var now = DateTime.UtcNow.Ticks;
                waitingSeconds = (int)((now - enqueuedAt) / TimeSpan.TicksPerSecond);
            }
            
            var currentDelta = CalculateDelta(waitingSeconds);
            
            return new PlayerQueueStatus(
                state.ToString(),
                string.IsNullOrEmpty(matchId) ? null : Guid.Parse(matchId),
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
        var db = _redis.GetDatabase();
        
        try
        {
            // Süresi dolmuş accept set'lerini bul
            var expiredAccepts = await db.ExecuteAsync("SCAN", "0", "MATCH", "mmrpg:pvp:accept:*", "COUNT", "100");
            
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
