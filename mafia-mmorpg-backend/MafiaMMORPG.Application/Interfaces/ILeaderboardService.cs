using MafiaMMORPG.Application.DTOs;

namespace MafiaMMORPG.Application.Interfaces;

public interface ILeaderboardService
{
    Task<IReadOnlyList<LeaderboardEntryDto>> GetGlobalTopAsync(int top = 1000, TimeSpan? ttl = null);
    Task<IReadOnlyList<LeaderboardEntryDto>> GetRegionalTopAsync(string region, int top = 1000, TimeSpan? ttl = null);
    Task InvalidateAsync();
}
