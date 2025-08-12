using MafiaMMORPG.Application.DTOs;

namespace MafiaMMORPG.Application.Interfaces;

public interface IMatchmakingService
{
    Task<MatchInfo?> EnqueueAsync(Guid playerId);
    Task<bool> DequeueAsync(Guid playerId);
    Task<bool> AcceptAsync(Guid playerId, Guid matchId);
    Task<PlayerQueueStatus?> GetStatusAsync(Guid playerId);
}
