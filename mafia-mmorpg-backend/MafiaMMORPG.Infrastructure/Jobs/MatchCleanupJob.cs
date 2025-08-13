using Quartz;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;

namespace MafiaMMORPG.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class MatchCleanupJob : IJob
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<MatchCleanupJob> _logger;

    public MatchCleanupJob(IConnectionMultiplexer redis, ILogger<MatchCleanupJob> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var db = _redis.GetDatabase();
            var server = _redis.GetServer(_redis.GetEndPoints().First());

            // Clean up expired match keys
            var matchKeys = server.Keys(pattern: "mmrpg:pvp:match:*").ToArray();
            var expiredMatchKeys = new List<RedisKey>();

            foreach (var key in matchKeys)
            {
                if (!await db.KeyExistsAsync(key))
                {
                    expiredMatchKeys.Add(key);
                }
            }

            if (expiredMatchKeys.Count > 0)
            {
                await db.KeyDeleteAsync(expiredMatchKeys.ToArray());
                _logger.LogInformation("Cleaned up {Count} expired match keys", expiredMatchKeys.Count);
            }

            // Clean up expired accept keys
            var acceptKeys = server.Keys(pattern: "mmrpg:pvp:accept:*").ToArray();
            var expiredAcceptKeys = new List<RedisKey>();

            foreach (var key in acceptKeys)
            {
                if (!await db.KeyExistsAsync(key))
                {
                    expiredAcceptKeys.Add(key);
                }
            }

            if (expiredAcceptKeys.Count > 0)
            {
                await db.KeyDeleteAsync(expiredAcceptKeys.ToArray());
                _logger.LogInformation("Cleaned up {Count} expired accept keys", expiredAcceptKeys.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during match cleanup job");
            throw;
        }
    }
}
