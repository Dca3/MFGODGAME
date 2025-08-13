export interface MatchInfo {
  matchId: string;
  p1Id: string;
  p2Id: string;
  createdAt: string;
  state: string;
}

export interface DuelSnapshot {
  matchId: string;
  turn: number;
  p1Hp: number;
  p2Hp: number;
  logLine: string;
}

export interface CombatResult {
  attackerId: string;
  defenderId: string;
  attackerHpLeft: number;
  defenderHpLeft: number;
  logJson: string;
  attackerWon: boolean;
}

export type QueueState = 'idle' | 'queued' | 'matched' | 'inDuel';
