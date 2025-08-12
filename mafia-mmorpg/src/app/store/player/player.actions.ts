import { createAction, props } from '@ngrx/store';

export interface PlayerStats {
  karizma: number;
  guc: number;
  zeka: number;
  hayat: number;
  freePoints: number;
}

export interface Player {
  id: string;
  username: string;
  level: number;
  experience: number;
  money: number;
  reputation: number;
  stats: PlayerStats;
  mmr: number;
  wins: number;
  losses: number;
}

// Load Player
export const loadPlayer = createAction('[Player] Load Player');
export const loadPlayerSuccess = createAction(
  '[Player] Load Player Success',
  props<{ player: Player }>()
);
export const loadPlayerFailure = createAction(
  '[Player] Load Player Failure',
  props<{ error: string }>()
);

// Update Stats
export const allocateStatPoint = createAction(
  '[Player] Allocate Stat Point',
  props<{ stat: keyof PlayerStats }>()
);
export const allocateStatPointSuccess = createAction(
  '[Player] Allocate Stat Point Success',
  props<{ stats: PlayerStats }>()
);
export const allocateStatPointFailure = createAction(
  '[Player] Allocate Stat Point Failure',
  props<{ error: string }>()
);

// Update Player
export const updatePlayer = createAction(
  '[Player] Update Player',
  props<{ updates: Partial<Player> }>()
);
export const updatePlayerSuccess = createAction(
  '[Player] Update Player Success',
  props<{ player: Player }>()
);
export const updatePlayerFailure = createAction(
  '[Player] Update Player Failure',
  props<{ error: string }>()
);
