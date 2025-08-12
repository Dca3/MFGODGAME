using MafiaMMORPG.Application.DTOs;

namespace MafiaMMORPG.Application.Interfaces;

public interface ICombatService
{
    CombatResult SimulatePve(CombatRequest req);   // NPC görev dövüşü
    CombatResult SimulatePvp(CombatRequest req);   // Düello (server-otoriter)
}
