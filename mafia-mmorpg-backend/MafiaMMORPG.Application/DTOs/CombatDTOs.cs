namespace MafiaMMORPG.Application.DTOs;

public record CombatRequest(Guid AttackerId, Guid DefenderId, Guid? DuelId = null);

public record CombatResult(Guid AttackerId, Guid DefenderId, double AttackerHpLeft,
                           double DefenderHpLeft, string LogJson, bool AttackerWon);

public record MatchInfo(Guid MatchId, Guid P1Id, Guid P2Id, DateTime CreatedAt, string State);

public record DuelSnapshot(Guid MatchId, int Turn, double P1Hp, double P2Hp, string LogLine);
