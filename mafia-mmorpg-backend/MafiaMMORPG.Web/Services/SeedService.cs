using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MafiaMMORPG.Infrastructure.Data;
using MafiaMMORPG.Application.Repositories;
using MafiaMMORPG.Domain.Entities;
using MafiaMMORPG.Domain.Enums;

namespace MafiaMMORPG.Web.Services;

public class SeedService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IRepository<Player> _playerRepo;
    private readonly IRepository<PlayerStats> _statsRepo;
    private readonly IRepository<Rating> _ratingRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public SeedService(
        ApplicationDbContext db,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IRepository<Player> playerRepo,
        IRepository<PlayerStats> statsRepo,
        IRepository<Rating> ratingRepo,
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
        _playerRepo = playerRepo;
        _statsRepo = statsRepo;
        _ratingRepo = ratingRepo;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        // Create roles
        await CreateRolesAsync();

        // Create admin user
        await CreateAdminUserAsync();

        // Create demo player
        await CreateDemoPlayerAsync();
        
        // Create quests and items
        await CreateQuestsAsync();
        await CreateItemDefinitionsAsync();
    }

    private async Task CreateRolesAsync()
    {
        var roles = new[] { "Player", "Admin" };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private async Task CreateAdminUserAsync()
    {
        var adminEmail = _configuration["Seed:AdminEmail"];
        var adminPassword = _configuration["Seed:AdminPassword"];

        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            return;

        var adminUser = await _userManager.FindByEmailAsync(adminEmail);
        if (adminUser != null)
            return;

        adminUser = new IdentityUser
        {
            UserName = "admin",
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }

    private async Task CreateDemoPlayerAsync()
    {
        var playerEmail = _configuration["Seed:PlayerEmail"];
        var playerPassword = _configuration["Seed:PlayerPassword"];

        if (string.IsNullOrEmpty(playerEmail) || string.IsNullOrEmpty(playerPassword))
            return;

        var playerUser = await _userManager.FindByEmailAsync(playerEmail);
        if (playerUser != null)
            return;

        playerUser = new IdentityUser
        {
            UserName = "demo_player",
            Email = playerEmail,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(playerUser, playerPassword);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(playerUser, "Player");

            // Create Player entity
            var player = new Player
            {
                Id = Guid.Parse(playerUser.Id),
                Username = playerUser.UserName!,
                Level = 1,
                Money = 1000,
                Reputation = 0,
                CreatedAt = DateTime.UtcNow
            };

            // Create PlayerStats
            var stats = new PlayerStats
            {
                PlayerId = player.Id,
                Karizma = 2,
                Guc = 1,
                Zeka = 1,
                Hayat = 1,
                FreePoints = 0
            };

            // Create Rating
            var rating = new Rating
            {
                PlayerId = player.Id,
                MMR = 1200,
                Wins = 0,
                Losses = 0
            };

            await _unitOfWork.ExecuteInTransactionAsync(async (ct) =>
            {
                await _playerRepo.AddAsync(player, ct);
                await _statsRepo.AddAsync(stats, ct);
                await _ratingRepo.AddAsync(rating, ct);
            });
        }
    }

    private async Task CreateQuestsAsync()
    {
        if (await _db.Quests.AnyAsync())
            return;

        var quests = new List<Quest>
        {
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Mahalleye Giriş",
                Description = "Yeni bir mahalleye giriş yap ve yerel mafya ile tanış.",
                Difficulty = QuestDifficulty.Easy,
                RequiredLevel = 1,
                Location = "Küçük Mahalle",
                NpcName = "Mahalle Ağası"
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Rüşvet Zinciri",
                Description = "Polis memurlarına rüşvet ver ve onları yanına çek.",
                Difficulty = QuestDifficulty.Normal,
                RequiredLevel = 3,
                Location = "Polis Karakolu",
                NpcName = "Komiser"
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Kasa Soygunu",
                Description = "Büyük bir bankanın kasasını soy ve değerli eşyaları çıkar.",
                Difficulty = QuestDifficulty.Hard,
                RequiredLevel = 5,
                Location = "Merkez Bankası",
                NpcName = "Güvenlik Şefi"
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Büyük Vurgun",
                Description = "Şehrin en büyük kumarhanesini soy ve tüm parayı al.",
                Difficulty = QuestDifficulty.Mythic,
                RequiredLevel = 8,
                Location = "Altın Kumarhane",
                NpcName = "Kumarhane Sahibi"
            }
        };

        await _db.Quests.AddRangeAsync(quests);
        await _db.SaveChangesAsync();
    }

    private async Task CreateItemDefinitionsAsync()
    {
        if (await _db.ItemDefinitions.AnyAsync())
            return;

        var items = new List<ItemDefinition>
        {
            // Common items
            new ItemDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Paslı Tabanca",
                Slot = ItemSlot.Weapon,
                Rarity = ItemRarity.Common,
                ItemLevel = 5,
                RequiredLevel = 1,
                BaseG = 3
            },
            new ItemDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Sıradan Gözlük",
                Slot = ItemSlot.Glasses,
                Rarity = ItemRarity.Common,
                ItemLevel = 8,
                RequiredLevel = 3,
                BaseZ = 2
            },
            new ItemDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Eski Smokin",
                Slot = ItemSlot.Suit,
                Rarity = ItemRarity.Common,
                ItemLevel = 10,
                RequiredLevel = 5,
                BaseK = 1
            },
            
            // Uncommon items
            new ItemDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Denge Tabancası",
                Slot = ItemSlot.Weapon,
                Rarity = ItemRarity.Uncommon,
                ItemLevel = 20,
                RequiredLevel = 12,
                BaseG = 8,
                BaseZ = 3
            },
            new ItemDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Keskin Bakış Gözlüğü",
                Slot = ItemSlot.Glasses,
                Rarity = ItemRarity.Uncommon,
                ItemLevel = 22,
                RequiredLevel = 15,
                BaseZ = 8
            },
            
            // Rare items
            new ItemDefinition
            {
                Id = Guid.NewGuid(),
                Name = "İmza Silahı",
                Slot = ItemSlot.Weapon,
                Rarity = ItemRarity.Rare,
                ItemLevel = 40,
                RequiredLevel = 28,
                BaseK = 8,
                BaseG = 5
            },
            
            // Epic items
            new ItemDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Gece Operatörü Smokin",
                Slot = ItemSlot.Suit,
                Rarity = ItemRarity.Epic,
                ItemLevel = 60,
                RequiredLevel = 45,
                BaseZ = 12,
                BaseK = 15
            },
            
            // Legendary items
            new ItemDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Gece Gölgesi Smokin",
                Slot = ItemSlot.Suit,
                Rarity = ItemRarity.Legendary,
                ItemLevel = 80,
                RequiredLevel = 60,
                BaseK = 25,
                BaseZ = 20
            },
            new ItemDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Patronun Bakışı Gözlük",
                Slot = ItemSlot.Glasses,
                Rarity = ItemRarity.Legendary,
                ItemLevel = 20,
                RequiredLevel = 20,
                BaseZ = 30
            }
        };

        await _db.ItemDefinitions.AddRangeAsync(items);
        await _db.SaveChangesAsync();
    }
}
