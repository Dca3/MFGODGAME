export interface PlayerProfileDto {
  id: string;
  username: string;
  level: number;
  experience: number;
  money: number;
  reputation: number;
  createdAt: string;
}

export interface PlayerStatsDto {
  Karizma: number;
  Guc: number;
  Zeka: number;
  Hayat: number;
  FreePoints: number;
}

export interface AllocateStatsRequest {
  Karizma: number;
  Guc: number;
  Zeka: number;
  Hayat: number;
}
