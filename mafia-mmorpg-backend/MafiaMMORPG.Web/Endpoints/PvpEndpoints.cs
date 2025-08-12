using System.Security.Claims;
using MafiaMMORPG.Infrastructure.Data;

namespace MafiaMMORPG.Web.Endpoints;

public static class PvpEndpoints
{
    public static IEndpointRouteBuilder MapPvpEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/pvp/queue", async (ClaimsPrincipal user, ApplicationDbContext db) =>
        {
            // TODO: Implement join queue logic
            return Results.Ok(new { Message = "Join queue endpoint" });
        })
        .WithName("JoinQueue")
        .WithOpenApi()
        .RequireAuthorization();

        app.MapDelete("/pvp/queue", async (ClaimsPrincipal user, ApplicationDbContext db) =>
        {
            // TODO: Implement leave queue logic
            return Results.Ok(new { Message = "Leave queue endpoint" });
        })
        .WithName("LeaveQueue")
        .WithOpenApi()
        .RequireAuthorization();

        app.MapGet("/pvp/status", async (ClaimsPrincipal user, ApplicationDbContext db) =>
        {
            // TODO: Implement get queue status logic
            return Results.Ok(new { Message = "Get queue status endpoint" });
        })
        .WithName("GetQueueStatus")
        .WithOpenApi()
        .RequireAuthorization();

        return app;
    }
}
