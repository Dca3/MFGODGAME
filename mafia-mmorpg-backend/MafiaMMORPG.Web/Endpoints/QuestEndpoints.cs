using System.Security.Claims;
using MafiaMMORPG.Infrastructure.Data;
using MafiaMMORPG.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MafiaMMORPG.Web.Endpoints;

public static class QuestEndpoints
{
    public static IEndpointRouteBuilder MapQuestEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/quests/available", async (ClaimsPrincipal user, IQuestService questService, ApplicationDbContext db) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var player = await db.Players.FirstOrDefaultAsync(p => p.UserId == userId);
            if (player == null)
                return Results.NotFound();

            var availableQuests = await questService.GetAvailableQuestsAsync(player.Id);
            return Results.Ok(availableQuests);
        })
        .WithName("GetAvailableQuests")
        .WithOpenApi()
        .RequireAuthorization();

        app.MapPost("/quests/{id}/start", async (Guid id, ClaimsPrincipal user, IQuestService questService, ApplicationDbContext db) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var player = await db.Players.FirstOrDefaultAsync(p => p.UserId == userId);
            if (player == null)
                return Results.NotFound();

            var success = await questService.StartQuestAsync(player.Id, id);
            if (!success)
                return Results.BadRequest(new { Message = "Failed to start quest" });

            return Results.Ok(new { Message = "Quest started successfully" });
        })
        .WithName("StartQuest")
        .WithOpenApi()
        .RequireAuthorization();

        app.MapPost("/quests/{id}/complete", async (Guid id, ClaimsPrincipal user, IQuestService questService, ApplicationDbContext db) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var player = await db.Players.FirstOrDefaultAsync(p => p.UserId == userId);
            if (player == null)
                return Results.NotFound();

            var result = await questService.CompleteQuestAsync(player.Id, id);
            return Results.Ok(new
            {
                success = result.Success,
                gainedXp = result.GainedXp,
                money = result.Money,
                rewardItemIds = result.RewardItemIds,
                newLevel = result.NewLevel,
                freePointsGained = result.FreePointsGained
            });
        })
        .WithName("CompleteQuest")
        .WithOpenApi()
        .RequireAuthorization();

        return app;
    }
}
