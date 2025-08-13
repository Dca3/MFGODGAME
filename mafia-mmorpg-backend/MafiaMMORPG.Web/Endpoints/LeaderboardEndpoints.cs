using MafiaMMORPG.Infrastructure.Data;
using MafiaMMORPG.Application.Interfaces;

namespace MafiaMMORPG.Web.Endpoints;

public static class LeaderboardEndpoints
{
    public static IEndpointRouteBuilder MapLeaderboardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/leaderboard/global", async (int? top, ILeaderboardService leaderboardService) =>
        {
            try
            {
                var leaderboard = await leaderboardService.GetGlobalTopAsync(top ?? 1000);
                return Results.Ok(leaderboard);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("GetGlobalLeaderboard")
        .WithOpenApi();

        app.MapGet("/leaderboard/region/{code}", async (string code, int? top, ILeaderboardService leaderboardService) =>
        {
            try
            {
                var leaderboard = await leaderboardService.GetRegionalTopAsync(code, top ?? 1000);
                return Results.Ok(leaderboard);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("GetRegionalLeaderboard")
        .WithOpenApi();

        return app;
    }
}
