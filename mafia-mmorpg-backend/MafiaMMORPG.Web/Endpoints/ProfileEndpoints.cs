using System.Security.Claims;
using MafiaMMORPG.Web.DTOs;
using MafiaMMORPG.Infrastructure.Data;
using MafiaMMORPG.Application.Repositories;
using MafiaMMORPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MafiaMMORPG.Web.Endpoints;

public static class ProfileEndpoints
{
    public static IEndpointRouteBuilder MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/me", async (ClaimsPrincipal user, ApplicationDbContext db) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var playerId))
                return Results.Unauthorized();

            var player = await db.Players
                .Where(p => p.Id == playerId)
                .Select(p => new { p.Id, p.Username, p.Level, p.Experience, p.Money, p.Reputation, p.CreatedAt })
                .FirstOrDefaultAsync();

            if (player == null)
                return Results.NotFound();

            return Results.Ok(player);
        })
        .WithName("GetProfile")
        .WithOpenApi()
        .RequireAuthorization();

        app.MapGet("/me/stats", async (ClaimsPrincipal user, ApplicationDbContext db) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var playerId))
                return Results.Unauthorized();

            var stats = await db.PlayerStats
                .Where(s => s.PlayerId == playerId)
                .Select(s => new { s.Karizma, s.Guc, s.Zeka, s.Hayat, s.FreePoints })
                .FirstOrDefaultAsync();

            if (stats == null)
                return Results.NotFound();

            return Results.Ok(stats);
        })
        .WithName("GetStats")
        .WithOpenApi()
        .RequireAuthorization();

        app.MapPost("/me/stats/allocate", async (AllocateStatsRequest request, ClaimsPrincipal user, ApplicationDbContext db, IRepository<PlayerStats> statsRepo, IUnitOfWork uow) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var playerId))
                return Results.Unauthorized();

            // Validate request
            if (request.Karizma < 0 || request.Guc < 0 || request.Zeka < 0 || request.Hayat < 0)
            {
                return Results.BadRequest(new { Message = "Stat values cannot be negative" });
            }

            var totalRequested = request.Karizma + request.Guc + request.Zeka + request.Hayat;
            if (totalRequested <= 0)
            {
                return Results.BadRequest(new { Message = "At least one stat must be increased" });
            }

            try
            {
                await uow.ExecuteInTransactionAsync(async (ct) =>
                {
                    var stats = await statsRepo.FirstOrDefaultAsync(s => s.PlayerId == playerId, asNoTracking: false, ct);
                    if (stats == null)
                        throw new InvalidOperationException("Player stats not found");

                    if (stats.FreePoints < totalRequested)
                        throw new InvalidOperationException($"Not enough free points. Available: {stats.FreePoints}, Requested: {totalRequested}");

                    // Allocate stats
                    stats.Karizma += request.Karizma;
                    stats.Guc += request.Guc;
                    stats.Zeka += request.Zeka;
                    stats.Hayat += request.Hayat;
                    stats.FreePoints -= totalRequested;

                    await statsRepo.UpdateAsync(stats, ct);
                });

                return Results.Ok(new { Message = "Stats allocated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("AllocateStats")
        .WithOpenApi()
        .RequireAuthorization();

        return app;
    }
}
