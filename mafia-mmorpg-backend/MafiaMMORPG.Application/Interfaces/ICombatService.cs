using MafiaMMORPG.Application.DTOs;

namespace MafiaMMORPG.Application.Interfaces;

public record PveSimulationResult(bool Success, string? ErrorMessage = null);

public interface ICombatService
{
    Task<PveSimulationResult> SimulatePveAsync(Guid playerId, Guid questId, CancellationToken ct = default);
}
