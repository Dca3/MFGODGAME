using StackExchange.Redis;
using System.Text.Json;
using MafiaMMORPG.Application.Interfaces;
using MafiaMMORPG.Application.DTOs;
using MafiaMMORPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MafiaMMORPG.Infrastructure.Services;

public class LeaderboardService : ILeaderboardService
{
    private readonly ApplicationDbContext _db;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<LeaderboardService> _logger;
    private readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(1);

    public LeaderboardService(
        ApplicationDbContext db,
        IConnectionMultiplexer redis,
        ILogger<LeaderboardService> logger)
    {
        _db = db;
        _redis = redis;
        _logger = logger;
    }

    public async Task<IReadOnlyList<LeaderboardEntryDto>> GetGlobalTopAsync(int top = 1000, TimeSpan? ttl = null)
    {
        var cacheKey = $"lb:global:top:{top}";
        var ttlValue = ttl ?? _defaultTtl;

        // Try cache first
        var cached = await GetFromCacheAsync(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("Cache hit for global leaderboard top {Top}", top);
            return cached;
        }

        // Get from database
        var ratings = await _db.Ratings
            .Include(r => r.Player)
            .OrderByDescending(r => r.MMR)
            .Take(top)
            .ToListAsync();

        var leaderboard = ratings.Select((r, index) => new LeaderboardEntryDto(
            r.PlayerId,
            index + 1,
            r.MMR,
            r.Player.Username,
            r.Player.Level,
            r.Player.Reputation
        )).ToList();

        // Cache the result
        await SetCacheAsync(cacheKey, leaderboard, ttlValue);
        _logger.LogInformation("Cache miss for global leaderboard top {Top}, cached {Count} entries", top, leaderboard.Count);

        return leaderboard;
    }

    public async Task<IReadOnlyList<LeaderboardEntryDto>> GetRegionalTopAsync(string region, int top = 1000, TimeSpan? ttl = null)
    {
        var cacheKey = $"lb:region:{region}:top:{top}";
        var ttlValue = ttl ?? _defaultTtl;

        // Try cache first
        var cached = await GetFromCacheAsync(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("Cache hit for regional leaderboard {Region} top {Top}", region, top);
            return cached;
        }

        // Get from database (for now, all players are in same region)
        // TODO: Add region field to Player entity
        var ratings = await _db.Ratings
            .Include(r => r.Player)
            .OrderByDescending(r => r.MMR)
            .Take(top)
            .ToListAsync();

        var leaderboard = ratings.Select((r, index) => new LeaderboardEntryDto(
            r.PlayerId,
            index + 1,
            r.MMR,
            r.Player.Username,
            r.Player.Level,
            r.Player.Reputation,
            region
        )).ToList();

        // Cache the result
        await SetCacheAsync(cacheKey, leaderboard, ttlValue);
        _logger.LogInformation("Cache miss for regional leaderboard {Region} top {Top}, cached {Count} entries", region, top, leaderboard.Count);

        return leaderboard;
    }

    public async Task InvalidateAsync()
    {
        var db = _redis.GetDatabase();
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        
        var keys = server.Keys(pattern: "lb:*").ToArray();
        if (keys.Length > 0)
        {
            await db.KeyDeleteAsync(keys);
            _logger.LogInformation("Invalidated {Count} leaderboard cache keys", keys.Length);
        }
    }

    private async Task<List<LeaderboardEntryDto>?> GetFromCacheAsync(string key)
    {
        try
        {
            var db = _redis.GetDatabase();
            var cached = await db.StringGetAsync(key);
            
            if (!cached.HasValue)
                return null;

            return JsonSerializer.Deserialize<List<LeaderboardEntryDto>>(cached!);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get leaderboard from cache for key {Key}", key);
            return null;
        }
    }

    private async Task SetCacheAsync(string key, List<LeaderboardEntryDto> data, TimeSpan ttl)
    {
        try
        {
            var db = _redis.GetDatabase();
            var json = JsonSerializer.Serialize(data);
            await db.StringSetAsync(key, json, ttl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set leaderboard cache for key {Key}", key);
        }
    }
}
