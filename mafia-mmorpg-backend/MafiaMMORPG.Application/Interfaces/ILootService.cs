using MafiaMMORPG.Domain.Enums;

namespace MafiaMMORPG.Application.Interfaces;

public interface ILootService
{
    // QuestDifficulty ve oyuncu leveline göre rarity seç, uygun banddan ItemDefinition üret/çek
    Task<(ItemRarity rarity, Guid itemDefinitionId)?> RollAsync(Guid playerId, Guid questId, QuestDifficulty diff, int playerLevel, CancellationToken ct = default);
}
