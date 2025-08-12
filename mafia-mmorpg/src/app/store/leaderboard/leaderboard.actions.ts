import { createAction, props } from '@ngrx/store';

export interface LeaderboardEntry {
  rank: number;
  playerId: string;
  username: string;
  mmr: number;
  wins: number;
  losses: number;
  winRate: number;
  level: number;
}

export interface Season {
  id: string;
  name: string;
  startDate: Date;
  endDate: Date;
  status: 'active' | 'ended' | 'upcoming';
  rewards: {
    top1000: string[];
    top100: string[];
    top10: string[];
  };
}

// Load Leaderboard
export const loadLeaderboard = createAction(
  '[Leaderboard] Load Leaderboard',
  props<{ leaderboardType: 'global' | 'regional'; region?: string }>()
);
export const loadLeaderboardSuccess = createAction(
  '[Leaderboard] Load Leaderboard Success',
  props<{ entries: LeaderboardEntry[]; leaderboardType: string }>()
);
export const loadLeaderboardFailure = createAction(
  '[Leaderboard] Load Leaderboard Failure',
  props<{ error: string }>()
);

// Load Seasons
export const loadSeasons = createAction('[Leaderboard] Load Seasons');
export const loadSeasonsSuccess = createAction(
  '[Leaderboard] Load Seasons Success',
  props<{ seasons: Season[] }>()
);
export const loadSeasonsFailure = createAction(
  '[Leaderboard] Load Seasons Failure',
  props<{ error: string }>()
);

// Current Season
export const loadCurrentSeason = createAction('[Leaderboard] Load Current Season');
export const loadCurrentSeasonSuccess = createAction(
  '[Leaderboard] Load Current Season Success',
  props<{ season: Season }>()
);
