using MafiaMMORPG.Domain.Entities;

namespace MafiaMMORPG.Application.Interfaces;

public record QuestCompleteResult(bool Success, int GainedXp, int Money, IReadOnlyList<Guid> RewardItemIds, int? NewLevel, int? FreePointsGained);

public interface IQuestService
{
    Task<IReadOnlyList<Quest>> GetAvailableQuestsAsync(Guid playerId, CancellationToken ct = default);
    Task<bool> StartQuestAsync(Guid playerId, Guid questId, CancellationToken ct = default);
    Task<QuestCompleteResult> CompleteQuestAsync(Guid playerId, Guid questId, CancellationToken ct = default);
}
