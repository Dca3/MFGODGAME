namespace MafiaMMORPG.Application.DTOs;

public record PlayerQueueStatus(string State, Guid? MatchId, int WaitingSeconds, int CurrentDelta);
