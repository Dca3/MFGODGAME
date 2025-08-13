export interface PlayerProfileDto {
  id: string;
  level: number;
  money: number;
  reputation: number;
  createdAt: string;
}

export interface PlayerStatsDto {
  k: number;
  g: number;
  z: number;
  h: number;
  freePoints: number;
}

export interface AllocateStatsRequest {
  k: number;
  g: number;
  z: number;
  h: number;
}
