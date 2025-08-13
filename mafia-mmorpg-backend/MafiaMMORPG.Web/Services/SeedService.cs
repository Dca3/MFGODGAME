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

         //Create admin user
        await CreateAdminUserAsync();

         //Create demo player
        await CreateDemoPlayerAsync();
        
        // Create quests and items
        await CreateQuestsAsync();
        //await CreateItemDefinitionsAsync();
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
            // Level 1-2 Quests (Easy)
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Mahalleye Giriş",
                Description = "Yeni bir mahalleye giriş yap ve yerel mafya ile tanış.",
                Difficulty = QuestDifficulty.Easy,
                RequiredLevel = 1,
                DurationMinutes = 15,
                CooldownMinutes = 30,
                Location = "Küçük Mahalle",
                NpcName = "Mahalle Ağası"
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Küçük Hırsızlık",
                Description = "Küçük bir dükkandan değerli eşyalar çal.",
                Difficulty = QuestDifficulty.Easy,
                RequiredLevel = 1,
                DurationMinutes = 20,
                CooldownMinutes = 45,
                Location = "Küçük Dükkan",
                NpcName = "Dükkan Sahibi"
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Koruma Parası Toplama",
                Description = "Küçük işletmelerden koruma parası topla.",
                Difficulty = QuestDifficulty.Easy,
                RequiredLevel = 1,
                DurationMinutes = 25,
                CooldownMinutes = 60,
                Location = "İş Merkezi",
                NpcName = "İş Adamı"
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Uyuşturucu Dağıtımı",
                Description = "Mahallede uyuşturucu dağıtımı yap.",
                Difficulty = QuestDifficulty.Easy,
                RequiredLevel = 1,
                DurationMinutes = 30,
                CooldownMinutes = 75,
                Location = "Karanlık Sokaklar",
                NpcName = "Uyuşturucu Satıcısı"
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Araba Hırsızlığı",
                Description = "Lüks bir araba çal ve parçala.",
                Difficulty = QuestDifficulty.Easy,
                RequiredLevel = 1,
                DurationMinutes = 35,
                CooldownMinutes = 90,
                Location = "Otopark",
                NpcName = "Otopark Görevlisi"
            },

            // Level 3+ Quests (Normal)
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Rüşvet Zinciri",
                Description = "Polis memurlarına rüşvet ver ve onları yanına çek.",
                Difficulty = QuestDifficulty.Normal,
                RequiredLevel = 3,
                DurationMinutes = 30,
                CooldownMinutes = 60,
                Location = "Polis Karakolu",
                NpcName = "Komiser"
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Koruma Parası",
                Description = "Küçük işletmelerden koruma parası topla.",
                Difficulty = QuestDifficulty.Normal,
                RequiredLevel = 3,
                DurationMinutes = 35,
                CooldownMinutes = 75,
                Location = "İş Merkezi",
                NpcName = "İş Adamı"
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Kumarhane Borcu",
                Description = "Kumarhane borçlarını tahsil et.",
                Difficulty = QuestDifficulty.Normal,
                RequiredLevel = 3,
                DurationMinutes = 40,
                CooldownMinutes = 90,
                Location = "Kumarhane",
                NpcName = "Kumarhane Sahibi"
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Silah Kaçakçılığı",
                Description = "Kaçak silahları şehre sok.",
                Difficulty = QuestDifficulty.Normal,
                RequiredLevel = 3,
                DurationMinutes = 45,
                CooldownMinutes = 105,
                Location = "Liman",
                NpcName = "Gümrük Memuru"
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Fuhuş Ağı",
                Description = "Fuhuş ağını genişlet ve yeni müşteriler bul.",
                Difficulty = QuestDifficulty.Normal,
                RequiredLevel = 3,
                DurationMinutes = 50,
                CooldownMinutes = 120,
                Location = "Gece Kulübü",
                NpcName = "Kulüp Sahibi"
            },

            // Level 5+ Quests (Hard)
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Kasa Soygunu",
                Description = "Büyük bir bankanın kasasını soy ve değerli eşyaları çıkar.",
                Difficulty = QuestDifficulty.Hard,
                RequiredLevel = 5,
                DurationMinutes = 45,
                CooldownMinutes = 90,
                Location = "Merkez Bankası",
                NpcName = "Güvenlik Şefi"
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Uyuşturucu Ticareti",
                Description = "Şehirde uyuşturucu ticareti yap ve büyük kâr elde et.",
                Difficulty = QuestDifficulty.Hard,
                RequiredLevel = 5,
                DurationMinutes = 50,
                CooldownMinutes = 120,
                Location = "Karanlık Sokaklar",
                NpcName = "Uyuşturucu Baronu"
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Cinayet Emri",
                Description = "Rakip mafya liderini öldür.",
                Difficulty = QuestDifficulty.Hard,
                RequiredLevel = 5,
                DurationMinutes = 55,
                CooldownMinutes = 150,
                Location = "Rakip Mahalle",
                NpcName = "Rakip Lider"
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Kumarhane Ele Geçirme",
                Description = "Rakip kumarhaneyi ele geçir.",
                Difficulty = QuestDifficulty.Hard,
                RequiredLevel = 5,
                DurationMinutes = 60,
                CooldownMinutes = 180,
                Location = "Rakip Kumarhane",
                NpcName = "Rakip Kumarhane Sahibi"
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Polis Karakolu Baskını",
                Description = "Polis karakoluna baskın düzenle ve dosyaları çal.",
                Difficulty = QuestDifficulty.Hard,
                RequiredLevel = 5,
                DurationMinutes = 65,
                CooldownMinutes = 210,
                Location = "Polis Karakolu",
                NpcName = "Polis Müdürü"
            },

            // Level 8+ Quests (Mythic)
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Büyük Vurgun",
                Description = "Şehrin en büyük kumarhanesini soy ve tüm parayı al.",
                Difficulty = QuestDifficulty.Mythic,
                RequiredLevel = 8,
                DurationMinutes = 60,
                CooldownMinutes = 180,
                Location = "Altın Kumarhane",
                NpcName = "Kumarhane Sahibi"
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Şehir Kontrolü",
                Description = "Tüm şehri kontrol altına al ve mafya imparatorluğu kur.",
                Difficulty = QuestDifficulty.Mythic,
                RequiredLevel = 8,
                DurationMinutes = 90,
                CooldownMinutes = 240,
                Location = "Şehir Merkezi",
                NpcName = "Şehir Valisi"
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Federal Bina Baskını",
                Description = "Federal binaya baskın düzenle ve gizli dosyaları çal.",
                Difficulty = QuestDifficulty.Mythic,
                RequiredLevel = 8,
                DurationMinutes = 120,
                CooldownMinutes = 300,
                Location = "Federal Bina",
                NpcName = "Federal Ajan"
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Rakip Mafya İmha",
                Description = "Tüm rakip mafya gruplarını imha et.",
                Difficulty = QuestDifficulty.Mythic,
                RequiredLevel = 8,
                DurationMinutes = 150,
                CooldownMinutes = 360,
                Location = "Rakip Bölgeler",
                NpcName = "Rakip Mafya Liderleri"
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Şehir Devrimi",
                Description = "Şehirde devrim çıkar ve kontrolü tamamen ele geçir.",
                Difficulty = QuestDifficulty.Mythic,
                RequiredLevel = 8,
                DurationMinutes = 180,
                CooldownMinutes = 480,
                Location = "Tüm Şehir",
                NpcName = "Şehir Halkı"
            },

            // Level 10+ Quests (Hard/Mythic)
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Banka Soygunu",
                Description = "Merkez bankasını soy ve tüm altınları çıkar.",
                Difficulty = QuestDifficulty.Hard,
                RequiredLevel = 10,
                DurationMinutes = 70,
                CooldownMinutes = 240,
                Location = "Merkez Bankası",
                NpcName = "Bank Müdürü"
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Hapishane Kaçışı",
                Description = "Hapishaneden kaç ve arkadaşlarını kurtar.",
                Difficulty = QuestDifficulty.Hard,
                RequiredLevel = 10,
                DurationMinutes = 75,
                CooldownMinutes = 270,
                Location = "Hapishane",
                NpcName = "Hapishane Müdürü"
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Nükleer Silah Kaçakçılığı",
                Description = "Nükleer silah parçalarını kaçak olarak getir.",
                Difficulty = QuestDifficulty.Mythic,
                RequiredLevel = 10,
                DurationMinutes = 200,
                CooldownMinutes = 600,
                Location = "Uluslararası Liman",
                NpcName = "Uluslararası Kaçakçı"
            },

            // Level 15+ Quests
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Devlet Başkanı Suikasti",
                Description = "Devlet başkanına suikast düzenle.",
                Difficulty = QuestDifficulty.Mythic,
                RequiredLevel = 15,
                DurationMinutes = 240,
                CooldownMinutes = 720,
                Location = "Başkanlık Sarayı",
                NpcName = "Devlet Başkanı"
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Şehir Nükleer Tehdidi",
                Description = "Şehri nükleer bomba ile tehdit et.",
                Difficulty = QuestDifficulty.Mythic,
                RequiredLevel = 15,
                DurationMinutes = 300,
                CooldownMinutes = 900,
                Location = "Şehir Merkezi",
                NpcName = "Şehir Halkı"
            },

            // Level 20+ Quests
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Ülke Kontrolü",
                Description = "Tüm ülkeyi kontrol altına al.",
                Difficulty = QuestDifficulty.Mythic,
                RequiredLevel = 20,
                DurationMinutes = 360,
                CooldownMinutes = 1200,
                Location = "Tüm Ülke",
                NpcName = "Ülke Halkı"
            },
            new Quest
            {
                Id = Guid.NewGuid(),
                Title = "Dünya Domination",
                Description = "Dünya çapında mafya imparatorluğu kur.",
                Difficulty = QuestDifficulty.Mythic,
                RequiredLevel = 20,
                DurationMinutes = 480,
                CooldownMinutes = 1440,
                Location = "Dünya Çapında",
                NpcName = "Dünya Liderleri"
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
