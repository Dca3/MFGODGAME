export interface LeaderboardEntryDto {
  playerId: string;
  rank: number;
  mmr: number;
  name?: string;
  level?: number;
  reputation?: number;
  region?: string;
}
