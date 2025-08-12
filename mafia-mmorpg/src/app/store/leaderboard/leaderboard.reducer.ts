import { createReducer, on } from '@ngrx/store';
import * as LeaderboardActions from './leaderboard.actions';

export interface LeaderboardState {
  entries: any[];
  seasons: any[];
  loading: boolean;
  error: string | null;
}

export const initialState: LeaderboardState = {
  entries: [],
  seasons: [],
  loading: false,
  error: null
};

export const leaderboardReducer = createReducer(
  initialState,
  on(LeaderboardActions.loadLeaderboard, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  on(LeaderboardActions.loadLeaderboardSuccess, (state, { entries }) => ({
    ...state,
    entries,
    loading: false
  })),
  on(LeaderboardActions.loadLeaderboardFailure, (state, { error }) => ({
    ...state,
    error,
    loading: false
  })),
  on(LeaderboardActions.loadSeasons, (state) => ({
    ...state,
    loading: true
  })),
  on(LeaderboardActions.loadSeasonsSuccess, (state, { seasons }) => ({
    ...state,
    seasons,
    loading: false
  })),
  on(LeaderboardActions.loadSeasonsFailure, (state, { error }) => ({
    ...state,
    error,
    loading: false
  }))
);
