using System.Security.Claims;
using MafiaMMORPG.Application.Interfaces;
using MafiaMMORPG.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace MafiaMMORPG.Web.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/seasons/open", async (ISeasonService seasonService, ClaimsPrincipal user, UserManager<IdentityUser> userManager) =>
        {
            // Check if user has Admin role
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var identityUser = await userManager.FindByIdAsync(userId);
            if (identityUser == null)
                return Results.Unauthorized();

            var roles = await userManager.GetRolesAsync(identityUser);
            if (!roles.Contains("Admin"))
                return Results.Forbid();

            try
            {
                var seasonId = await seasonService.OpenNextSeasonAsync();
                return Results.Ok(new { SeasonId = seasonId, Message = "New season opened successfully" });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("OpenSeason")
        .WithOpenApi()
        .RequireAuthorization();

        app.MapPost("/admin/seasons/{id}/close", async (Guid id, ISeasonService seasonService, ClaimsPrincipal user, UserManager<IdentityUser> userManager) =>
        {
            // Check if user has Admin role
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var identityUser = await userManager.FindByIdAsync(userId);
            if (identityUser == null)
                return Results.Unauthorized();

            var roles = await userManager.GetRolesAsync(identityUser);
            if (!roles.Contains("Admin"))
                return Results.Forbid();

            try
            {
                await seasonService.CloseSeasonAsync(id);
                return Results.Ok(new { Message = "Season closed successfully" });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("CloseSeason")
        .WithOpenApi()
        .RequireAuthorization();

        return app;
    }
}
