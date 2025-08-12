using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MafiaMMORPG.Infrastructure.Data;
using MafiaMMORPG.Application.Repositories;
using MafiaMMORPG.Domain.Entities;

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
}
