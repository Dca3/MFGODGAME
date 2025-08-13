using MafiaMMORPG.Application.Interfaces;
using MafiaMMORPG.Infrastructure.Data;
using MafiaMMORPG.Application.Repositories;
using MafiaMMORPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MafiaMMORPG.Infrastructure.Services;

public class SeasonService : ISeasonService
{
    private readonly ApplicationDbContext _db;
    private readonly IRepository<Season> _seasonRepo;
    private readonly IRepository<Leaderboard> _leaderboardRepo;
    private readonly IRepository<PlayerInventory> _inventoryRepo;
    private readonly IRepository<Rating> _ratingRepo;
    private readonly ILeaderboardService _leaderboardService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SeasonService> _logger;

    public SeasonService(
        ApplicationDbContext db,
        IRepository<Season> seasonRepo,
        IRepository<Leaderboard> leaderboardRepo,
        IRepository<PlayerInventory> inventoryRepo,
        IRepository<Rating> ratingRepo,
        ILeaderboardService leaderboardService,
        IUnitOfWork unitOfWork,
        ILogger<SeasonService> logger)
    {
        _db = db;
        _seasonRepo = seasonRepo;
        _leaderboardRepo = leaderboardRepo;
        _inventoryRepo = inventoryRepo;
        _ratingRepo = ratingRepo;
        _leaderboardService = leaderboardService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task CloseSeasonAsync(Guid seasonId)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async (ct) =>
        {
            // Get top 1000 players
            var topPlayers = await _db.Ratings
                .Include(r => r.Player)
                .OrderByDescending(r => r.MMR)
                .Take(1000)
                .ToListAsync(ct);

            // Create leaderboard entries
            var leaderboardEntries = new List<Leaderboard>();
            for (int i = 0; i < topPlayers.Count; i++)
            {
                var player = topPlayers[i];
                leaderboardEntries.Add(new Leaderboard
                {
                    SeasonId = seasonId,
                    PlayerId = player.PlayerId,
                    Rank = i + 1,
                    MMRSnapshot = player.MMR
                });
            }

            // Save leaderboard entries
            foreach (var entry in leaderboardEntries)
            {
                await _leaderboardRepo.AddAsync(entry, ct);
            }

            // Distribute rewards to top 1000
            await DistributeSeasonRewardsAsync(topPlayers, ct);

            // Soft reset MMR for all players
            await SoftResetMMRAsync(ct);

            // Mark season as closed
            var season = await _seasonRepo.FirstOrDefaultAsync(s => s.Id == seasonId, asNoTracking: false, ct);
            if (season != null)
            {
                season.Status = SeasonStatus.Ended;
                season.EndDate = DateTime.UtcNow;
                await _seasonRepo.UpdateAsync(season, ct);
            }

            _logger.LogInformation("Season {SeasonId} closed successfully. Top {Count} players recorded.", seasonId, topPlayers.Count);
        });

        // Invalidate leaderboard cache
        await _leaderboardService.InvalidateAsync();
    }

    public async Task<Guid> OpenNextSeasonAsync()
    {
        var seasonId = Guid.NewGuid();
        
        await _unitOfWork.ExecuteInTransactionAsync(async (ct) =>
        {
            var newSeason = new Season
            {
                Id = seasonId,
                Name = $"Season {DateTime.UtcNow.Year}-{DateTime.UtcNow.Month:D2}",
                Status = SeasonStatus.Active,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                RewardsJson = "{}" // TODO: Configure season rewards
            };

            await _seasonRepo.AddAsync(newSeason, ct);
        });

        _logger.LogInformation("New season {SeasonId} opened successfully.", seasonId);
        return seasonId;
    }

    public async Task<Guid> GetCurrentSeasonIdAsync()
    {
        var currentSeason = await _db.Seasons
            .Where(s => s.Status == SeasonStatus.Active)
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync();

        if (currentSeason == null)
        {
            // Create initial season if none exists
            return await OpenNextSeasonAsync();
        }

        return currentSeason.Id;
    }

    private async Task DistributeSeasonRewardsAsync(List<Rating> topPlayers, CancellationToken ct)
    {
        // Create season reward items
        var seasonRewards = new List<PlayerInventory>();

        for (int i = 0; i < Math.Min(topPlayers.Count, 1000); i++)
        {
            var player = topPlayers[i];
            var rank = i + 1;

            // Create cosmetic item based on rank
            var item = new Item
            {
                Id = Guid.NewGuid(),
                Name = GetSeasonRewardName(rank),
                Slot = "Cosmetic",
                Rarity = GetSeasonRewardRarity(rank),
                TagsJson = "{}"
            };

            var playerInventory = new PlayerInventory
            {
                Id = Guid.NewGuid(),
                PlayerId = player.PlayerId,
                ItemId = item.Id,
                IsEquipped = false,
                RollDataJson = "{}"
            };

            seasonRewards.Add(playerInventory);
        }

        // Save rewards
        foreach (var reward in seasonRewards)
        {
            await _inventoryRepo.AddAsync(reward, ct);
        }

        _logger.LogInformation("Distributed {Count} season rewards", seasonRewards.Count);
    }

    private async Task SoftResetMMRAsync(CancellationToken ct)
    {
        var allRatings = await _ratingRepo.GetAllAsync(ct);
        
        foreach (var rating in allRatings)
        {
            // Soft reset: mmr = 1200 + (mmr - 1200) * 0.5
            var newMMR = 1200 + (int)((rating.MMR - 1200) * 0.5);
            rating.MMR = Math.Max(1000, newMMR); // Minimum 1000 MMR
        }

        await _ratingRepo.UpdateRangeAsync(allRatings, ct);
        _logger.LogInformation("Soft reset MMR for {Count} players", allRatings.Count());
    }

    private static string GetSeasonRewardName(int rank)
    {
        return rank switch
        {
            1 => "Champion's Smoking Jacket",
            2 => "Vice Champion's Tie",
            3 => "Bronze Medalist's Cufflinks",
            <= 10 => "Top 10 Elite Badge",
            <= 100 => "Top 100 Achievement Pin",
            <= 1000 => "Season Participant Medal",
            _ => "Participation Certificate"
        };
    }

    private static ItemRarity GetSeasonRewardRarity(int rank)
    {
        return rank switch
        {
            1 => ItemRarity.Legendary,
            2 => ItemRarity.Epic,
            3 => ItemRarity.Rare,
            <= 10 => ItemRarity.Rare,
            <= 100 => ItemRarity.Rare,
            _ => ItemRarity.Common
        };
    }
}
