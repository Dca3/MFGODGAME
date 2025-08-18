using System.Security.Claims;
using MafiaMMORPG.Web.Services;
using MafiaMMORPG.Web.DTOs;
using MafiaMMORPG.Infrastructure.Data;
using MafiaMMORPG.Application.Repositories;
using MafiaMMORPG.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MafiaMMORPG.Web.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", async (RegisterRequest request, JwtService jwtService, ApplicationDbContext db, UserManager<IdentityUser> userManager, IRepository<Player> playerRepo, IRepository<PlayerStats> statsRepo, IRepository<Rating> ratingRepo, IUnitOfWork uow, HttpContext httpContext) =>
        {
            try
            {
                // Check if user already exists
                var existingUser = await userManager.FindByEmailAsync(request.Email) ?? 
                                  await userManager.FindByNameAsync(request.Username);
                if (existingUser != null)
                {
                    return Results.Conflict(new { Message = "User already exists" });
                }

                // Create Identity user
                var user = new IdentityUser
                {
                    UserName = request.Username,
                    Email = request.Email,
                    EmailConfirmed = true // Dev için
                };

                var result = await userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    return Results.BadRequest(new { Message = "Failed to create user", Errors = result.Errors });
                }

                // Create Player
                var player = new Player
                {
                    Id = Guid.Parse(user.Id),
                    UserId = user.Id,
                    Username = user.UserName!,
                    Level = 1,
                    Money = 1000,
                    Reputation = 0,
                    CreatedAt = DateTime.UtcNow
                };

                // Create PlayerStats
                var stats = new PlayerStats
                {
                    PlayerId = player.Id,
                    Karizma = 5,
                    Guc = 5,
                    Zeka = 5,
                    Hayat = 5,
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

                await uow.ExecuteInTransactionAsync(async (ct) =>
                {
                    await playerRepo.AddAsync(player, ct);
                    await statsRepo.AddAsync(stats, ct);
                    await ratingRepo.AddAsync(rating, ct);
                });

                // Generate tokens
                var accessToken = jwtService.GenerateAccessToken(user.Id, user.UserName!);
                var refreshToken = jwtService.GenerateRefreshToken();
                var expiresAt = DateTime.UtcNow.AddDays(7);

                // Save refresh token
                var refreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiresAt = expiresAt,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
                };

                await db.RefreshTokens.AddAsync(refreshTokenEntity);
                await db.SaveChangesAsync();

                return Results.Ok(new AuthResponse(accessToken, refreshToken, expiresAt));
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("Register")
        .WithOpenApi()
        .RequireRateLimiting("AuthPolicy");

        app.MapPost("/auth/login", async (LoginRequest request, JwtService jwtService, ApplicationDbContext db, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, HttpContext httpContext) =>
        {
            try
            {
                // Find user by email or username
                var user = await userManager.FindByEmailAsync(request.Username) ?? 
                          await userManager.FindByNameAsync(request.Username);

                if (user == null)
                {
                    return Results.Unauthorized();
                }

                // Check password
                var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (!result.Succeeded)
                {
                    return Results.Unauthorized();
                }

                // Revoke existing refresh tokens (optional)
                var existingTokens = await db.RefreshTokens
                    .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
                    .ToListAsync();

                foreach (var token in existingTokens)
                {
                    token.RevokedAt = DateTime.UtcNow;
                    token.RevokedByIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                }

                // Generate new tokens
                var accessToken = jwtService.GenerateAccessToken(user.Id, user.UserName!);
                var refreshToken = jwtService.GenerateRefreshToken();
                var expiresAt = DateTime.UtcNow.AddDays(7);

                // Save refresh token
                var refreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiresAt = expiresAt,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
                };

                await db.RefreshTokens.AddAsync(refreshTokenEntity);
                await db.SaveChangesAsync();

                return Results.Ok(new AuthResponse(accessToken, refreshToken, expiresAt));
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("Login")
        .WithOpenApi()
        .RequireRateLimiting("AuthPolicy");

        app.MapPost("/auth/refresh", async (RefreshRequest request, JwtService jwtService, ApplicationDbContext db, UserManager<IdentityUser> userManager, HttpContext httpContext) =>
        {
            try
            {
                // Find refresh token
                var refreshToken = await db.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

                if (refreshToken == null || refreshToken.RevokedAt != null || refreshToken.ExpiresAt <= DateTime.UtcNow)
                {
                    return Results.Unauthorized();
                }

                // Get user
                var user = await userManager.FindByIdAsync(refreshToken.UserId);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                // Revoke old token
                refreshToken.RevokedAt = DateTime.UtcNow;
                refreshToken.RevokedByIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                // Generate new tokens
                var accessToken = jwtService.GenerateAccessToken(user.Id, user.UserName!);
                var newRefreshToken = jwtService.GenerateRefreshToken();
                var expiresAt = DateTime.UtcNow.AddDays(7);

                // Link old token to new one
                refreshToken.ReplacedByToken = newRefreshToken;

                // Save new refresh token
                var newRefreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = newRefreshToken,
                    ExpiresAt = expiresAt,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
                };

                await db.RefreshTokens.AddAsync(newRefreshTokenEntity);
                await db.SaveChangesAsync();

                return Results.Ok(new AuthResponse(accessToken, newRefreshToken, expiresAt));
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("Refresh")
        .WithOpenApi()
        .RequireRateLimiting("AuthPolicy");

        return app;
    }
}
