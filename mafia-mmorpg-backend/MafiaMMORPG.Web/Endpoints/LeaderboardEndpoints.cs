using MafiaMMORPG.Infrastructure.Data;

namespace MafiaMMORPG.Web.Endpoints;

public static class LeaderboardEndpoints
{
    public static IEndpointRouteBuilder MapLeaderboardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/leaderboard/global", async (ApplicationDbContext db) =>
        {
            // TODO: Implement get global leaderboard logic
            return Results.Ok(new { Message = "Get global leaderboard endpoint" });
        })
        .WithName("GetGlobalLeaderboard")
        .WithOpenApi();

        app.MapGet("/leaderboard/region/{code}", async (string code, ApplicationDbContext db) =>
        {
            // TODO: Implement get regional leaderboard logic
            return Results.Ok(new { Message = "Get regional leaderboard endpoint" });
        })
        .WithName("GetRegionalLeaderboard")
        .WithOpenApi();

        return app;
    }
}
