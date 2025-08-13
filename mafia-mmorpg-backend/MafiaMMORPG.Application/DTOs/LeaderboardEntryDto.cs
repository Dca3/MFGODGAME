namespace MafiaMMORPG.Application.DTOs;

public record LeaderboardEntryDto(
    Guid PlayerId,
    int Rank,
    int MMR,
    string Name,
    int Level,
    int Reputation,
    string? Region = null
);
