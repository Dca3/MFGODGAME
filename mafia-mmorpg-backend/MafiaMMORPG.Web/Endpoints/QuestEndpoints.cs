using System.Security.Claims;
using MafiaMMORPG.Infrastructure.Data;

namespace MafiaMMORPG.Web.Endpoints;

public static class QuestEndpoints
{
    public static IEndpointRouteBuilder MapQuestEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/quests/available", async (ClaimsPrincipal user, ApplicationDbContext db) =>
        {
            // TODO: Implement get available quests logic
            return Results.Ok(new { Message = "Get available quests endpoint" });
        })
        .WithName("GetAvailableQuests")
        .WithOpenApi()
        .RequireAuthorization();

        app.MapPost("/quests/{id}/start", async (Guid id, ClaimsPrincipal user, ApplicationDbContext db) =>
        {
            // TODO: Implement start quest logic
            return Results.Ok(new { Message = "Start quest endpoint" });
        })
        .WithName("StartQuest")
        .WithOpenApi()
        .RequireAuthorization();

        app.MapPost("/quests/{id}/complete", async (Guid id, ClaimsPrincipal user, ApplicationDbContext db) =>
        {
            // TODO: Implement complete quest logic
            return Results.Ok(new { Message = "Complete quest endpoint" });
        })
        .WithName("CompleteQuest")
        .WithOpenApi()
        .RequireAuthorization();

        return app;
    }
}
