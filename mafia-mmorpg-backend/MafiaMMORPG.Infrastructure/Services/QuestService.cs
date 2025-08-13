using Microsoft.EntityFrameworkCore;
using MafiaMMORPG.Application.Interfaces;
using MafiaMMORPG.Domain.Entities;
using MafiaMMORPG.Domain.Enums;
using MafiaMMORPG.Infrastructure.Data;

namespace MafiaMMORPG.Infrastructure.Services;

public class QuestService : IQuestService
{
    private readonly ApplicationDbContext _db;
    private readonly IProgressionService _progressionService;
    private readonly ILootService _lootService;
    private readonly ICombatService _combatService;

    public QuestService(
        ApplicationDbContext db,
        IProgressionService progressionService,
        ILootService lootService,
        ICombatService combatService)
    {
        _db = db;
        _progressionService = progressionService;
        _lootService = lootService;
        _combatService = combatService;
    }

    public async Task<IReadOnlyList<Quest>> GetAvailableQuestsAsync(Guid playerId, CancellationToken ct = default)
    {
        var player = await _db.Players
            .Include(p => p.Quests)
            .FirstOrDefaultAsync(p => p.Id == playerId, ct);

        if (player == null)
            return new List<Quest>();

        var allQuests = await _db.Quests.ToListAsync(ct);
        var availableQuests = new List<Quest>();

        foreach (var quest in allQuests)
        {
            if (quest.RequiredLevel > player.Level)
                continue;

            var existingQuest = player.Quests.FirstOrDefault(pq => pq.QuestId == quest.Id);
            if (existingQuest?.State == PlayerQuestState.Completed)
                continue;

            if (existingQuest?.State == PlayerQuestState.Active)
                continue;

            availableQuests.Add(quest);
        }

        return availableQuests;
    }

    public async Task<bool> StartQuestAsync(Guid playerId, Guid questId, CancellationToken ct = default)
    {
        var player = await _db.Players
            .Include(p => p.Quests)
            .FirstOrDefaultAsync(p => p.Id == playerId, ct);

        if (player == null)
            return false;

        var quest = await _db.Quests.FindAsync(new object[] { questId }, ct);
        if (quest == null)
            return false;

        var existingQuest = player.Quests.FirstOrDefault(pq => pq.QuestId == questId);
        if (existingQuest?.State == PlayerQuestState.Active)
            return false;

        if (existingQuest == null)
        {
            existingQuest = new PlayerQuest
            {
                PlayerId = playerId,
                QuestId = questId,
                State = PlayerQuestState.Active,
                StartedAt = DateTime.UtcNow
            };
            _db.PlayerQuests.Add(existingQuest);
        }
        else
        {
            existingQuest.State = PlayerQuestState.Active;
            existingQuest.StartedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<QuestCompleteResult> CompleteQuestAsync(Guid playerId, Guid questId, CancellationToken ct = default)
    {
        var player = await _db.Players
            .Include(p => p.Quests)
            .Include(p => p.Stats)
            .FirstOrDefaultAsync(p => p.Id == playerId, ct);

        if (player == null)
            return new QuestCompleteResult(false, 0, 0, new List<Guid>(), null, null);

        var quest = await _db.Quests.FindAsync(new object[] { questId }, ct);
        if (quest == null)
            return new QuestCompleteResult(false, 0, 0, new List<Guid>(), null, null);

        var playerQuest = player.Quests.FirstOrDefault(pq => pq.QuestId == questId);
        if (playerQuest?.State != PlayerQuestState.Active)
            return new QuestCompleteResult(false, 0, 0, new List<Guid>(), null, null);

        var combatResult = await _combatService.SimulatePveAsync(playerId, questId, ct);
        if (!combatResult.Success)
        {
            playerQuest.State = PlayerQuestState.Failed;
            await _db.SaveChangesAsync(ct);
            return new QuestCompleteResult(false, 0, 0, new List<Guid>(), null, null);
        }

        var (gainedXp, money) = GetQuestRewards(quest.Difficulty);
        var rewardItemIds = new List<Guid>();

        var lootResult = await _lootService.RollAsync(playerId, questId, quest.Difficulty, player.Level, ct);
        if (lootResult.HasValue)
        {
            var (rarity, itemDefinitionId) = lootResult.Value;
            
            var inventoryItem = new PlayerInventory
            {
                PlayerId = playerId,
                ItemDefinitionId = itemDefinitionId,
                IsEquipped = false
            };
            _db.PlayerInventories.Add(inventoryItem);
            rewardItemIds.Add(itemDefinitionId);
        }

        var currentLevel = player.Level;
        var currentXp = player.Experience;
        var (newLevel, gainedFreePoints) = _progressionService.ApplyXp(
            ref currentLevel, 
            ref currentXp, 
            gainedXp, 
            5,
            100);
        
        player.Level = currentLevel;
        player.Experience = currentXp;

        if (gainedFreePoints > 0)
        {
            player.Stats.FreePoints += gainedFreePoints;
        }

        player.Money += money;
        playerQuest.State = PlayerQuestState.Completed;
        playerQuest.CompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return new QuestCompleteResult(
            true, 
            gainedXp, 
            money, 
            rewardItemIds, 
            newLevel > player.Level ? newLevel : null,
            gainedFreePoints > 0 ? gainedFreePoints : null);
    }

    private (int xp, int money) GetQuestRewards(QuestDifficulty difficulty)
    {
        return difficulty switch
        {
            QuestDifficulty.Easy => (120, 150),
            QuestDifficulty.Normal => (220, 300),
            QuestDifficulty.Hard => (380, 600),
            QuestDifficulty.Mythic => (650, 1200),
            _ => (100, 100)
        };
    }
}
