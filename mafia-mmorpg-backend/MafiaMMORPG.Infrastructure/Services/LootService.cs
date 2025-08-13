using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MafiaMMORPG.Application.Interfaces;
using MafiaMMORPG.Domain.Entities;
using MafiaMMORPG.Domain.Enums;
using MafiaMMORPG.Infrastructure.Data;

namespace MafiaMMORPG.Infrastructure.Services;

public class LootService : ILootService
{
    private readonly ApplicationDbContext _db;
    private readonly LootOptions _options;

    public LootService(ApplicationDbContext db, IOptions<LootOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public async Task<(ItemRarity rarity, Guid itemDefinitionId)?> RollAsync(Guid playerId, Guid questId, QuestDifficulty diff, int playerLevel, CancellationToken ct = default)
    {
        // Ultra-nadir Legendary kontrolü
        if (_options.UseUInt64Rng)
        {
            var threshold = (ulong)Math.Floor(_options.LegendaryAbsoluteProbability * ulong.MaxValue);
            Span<byte> b = stackalloc byte[8];
            RandomNumberGenerator.Fill(b);
            ulong r = BitConverter.ToUInt64(b);
            
            if (r <= threshold)
            {
                // Legendary drop!
                var legendaryItem = await GetRandomItemByRarityAndLevel(ItemRarity.Legendary, playerLevel, ct);
                if (legendaryItem != null)
                    return (ItemRarity.Legendary, legendaryItem.Id);
            }
        }

        // Normal rarity table
        var difficultyKey = diff.ToString();
        if (!_options.RarityRates.ContainsKey(difficultyKey))
            return null;

        var rates = _options.RarityRates[difficultyKey];
        var rarity = SelectRarityFromTable(rates);
        
        if (rarity == null)
            return null;

        var item = await GetRandomItemByRarityAndLevel(rarity.Value, playerLevel, ct);
        if (item == null)
            return null;

        return (rarity.Value, item.Id);
    }

    private ItemRarity? SelectRarityFromTable(Dictionary<string, double> rates)
    {
        var random = Random.Shared.NextDouble();
        double cumulative = 0;

        foreach (var rate in rates)
        {
            cumulative += rate.Value;
            if (random <= cumulative)
            {
                return Enum.Parse<ItemRarity>(rate.Key);
            }
        }

        return null;
    }

    private async Task<ItemDefinition?> GetRandomItemByRarityAndLevel(ItemRarity rarity, int playerLevel, CancellationToken ct)
    {
        if (!_options.RarityLevelBands.ContainsKey(rarity.ToString()))
            return null;

        var band = _options.RarityLevelBands[rarity.ToString()];
        
        // Oyuncu leveline yakın bir item level seç
        var targetItemLevel = Math.Max(band.Min, Math.Min(band.Max, playerLevel));
        
        // ±5 level tolerans
        var minLevel = Math.Max(band.Min, targetItemLevel - 5);
        var maxLevel = Math.Min(band.Max, targetItemLevel + 5);

        var items = await _db.ItemDefinitions
            .Where(i => i.Rarity == rarity && i.ItemLevel >= minLevel && i.ItemLevel <= maxLevel)
            .ToListAsync(ct);

        if (!items.Any())
            return null;

        // Rastgele seç
        var randomIndex = Random.Shared.Next(items.Count);
        return items[randomIndex];
    }
}

public class LootOptions
{
    public bool UseUInt64Rng { get; set; } = true;
    public double LegendaryAbsoluteProbability { get; set; } = 0.000000000004;
    public Dictionary<string, Dictionary<string, double>> RarityRates { get; set; } = new();
    public Dictionary<string, LevelBand> RarityLevelBands { get; set; } = new();
}

public class LevelBand
{
    public int Min { get; set; }
    public int Max { get; set; }
}
