using System.Security.Claims;
using MafiaMMORPG.Application.Repositories;
using MafiaMMORPG.Domain.Entities;
using MafiaMMORPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MafiaMMORPG.Web.Endpoints;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/me/inventory", async (ClaimsPrincipal user, ApplicationDbContext db) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var player = await db.Players.FirstOrDefaultAsync(p => p.UserId == userId);
            if (player == null)
                return Results.NotFound();

            var inventory = await db.PlayerInventories
                .Include(pi => pi.ItemDefinition)
                .Where(pi => pi.PlayerId == player.Id)
                .Select(pi => new
                {
                    pi.Id,
                    pi.ItemDefinitionId,
                    pi.ItemDefinition.Name,
                    pi.ItemDefinition.Slot,
                    pi.ItemDefinition.Rarity,
                    pi.ItemDefinition.ItemLevel,
                    pi.ItemDefinition.RequiredLevel,
                    pi.ItemDefinition.BaseK,
                    pi.ItemDefinition.BaseG,
                    pi.ItemDefinition.BaseZ,
                    pi.ItemDefinition.BaseH,
                    pi.IsEquipped
                })
                .ToListAsync();

            return Results.Ok(inventory);
        })
        .WithName("GetInventory")
        .WithOpenApi()
        .RequireAuthorization();

        app.MapPost("/me/items/equip", async (Guid itemId, ClaimsPrincipal user, IRepository<PlayerInventory> inventoryRepo, IRepository<Player> playerRepo, IUnitOfWork uow) =>
        {
            // TODO: Implement equip item logic with Repository Pattern
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var playerId))
                return Results.Unauthorized();

            await uow.ExecuteInTransactionAsync(async (ct) =>
            {
                // Get player inventory item
                var inventoryItem = await inventoryRepo.FirstOrDefaultAsync(
                    pi => pi.PlayerId == playerId && pi.ItemDefinitionId == itemId, 
                    asNoTracking: false, 
                    ct);

                if (inventoryItem == null)
                    throw new InvalidOperationException("Item not found in inventory");

                // Unequip current item in same slot
                var currentEquipped = await inventoryRepo.FirstOrDefaultAsync(
                    pi => pi.PlayerId == playerId && pi.IsEquipped && pi.ItemDefinition.Slot == inventoryItem.ItemDefinition.Slot,
                    asNoTracking: false,
                    ct);

                if (currentEquipped != null)
                {
                    currentEquipped.IsEquipped = false;
                    await inventoryRepo.UpdateAsync(currentEquipped, ct);
                }

                // Equip new item
                inventoryItem.IsEquipped = true;
                await inventoryRepo.UpdateAsync(inventoryItem, ct);
            });

            return Results.Ok(new { Message = "Item equipped successfully" });
        })
        .WithName("EquipItem")
        .WithOpenApi()
        .RequireAuthorization();

        app.MapPost("/me/items/unequip", async (Guid itemId, ClaimsPrincipal user, IRepository<PlayerInventory> inventoryRepo, IRepository<Player> playerRepo, IUnitOfWork uow) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var player = await playerRepo.FirstOrDefaultAsync(p => p.UserId == userId);
            if (player == null)
                return Results.NotFound();

            await uow.ExecuteInTransactionAsync(async (ct) =>
            {
                var inventoryItem = await inventoryRepo.FirstOrDefaultAsync(
                    pi => pi.PlayerId == player.Id && pi.ItemDefinitionId == itemId, 
                    asNoTracking: false, 
                    ct);

                if (inventoryItem == null)
                    throw new InvalidOperationException("Item not found in inventory");

                inventoryItem.IsEquipped = false;
                await inventoryRepo.UpdateAsync(inventoryItem, ct);
            });

            return Results.Ok(new { Message = "Item unequipped successfully" });
        })
        .WithName("UnequipItem")
        .WithOpenApi()
        .RequireAuthorization();

        app.MapPost("/craft/upgrade", (Guid itemId, ClaimsPrincipal user, ApplicationDbContext db) =>
        {
            // TODO: Implement craft upgrade logic
            return Results.Ok(new { Message = "Craft upgrade endpoint" });
        })
        .WithName("CraftUpgrade")
        .WithOpenApi()
        .RequireAuthorization();

        return app;
    }
}
