import { createReducer, on } from '@ngrx/store';
import * as PlayerActions from './player.actions';

export interface PlayerState {
  player: any | null;
  loading: boolean;
  error: string | null;
}

export const initialState: PlayerState = {
  player: null,
  loading: false,
  error: null
};

export const playerReducer = createReducer(
  initialState,
  on(PlayerActions.loadPlayer, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  on(PlayerActions.loadPlayerSuccess, (state, { player }) => ({
    ...state,
    player,
    loading: false
  })),
  on(PlayerActions.loadPlayerFailure, (state, { error }) => ({
    ...state,
    error,
    loading: false
  })),
  on(PlayerActions.allocateStatPoint, (state) => ({
    ...state,
    loading: true
  })),
  on(PlayerActions.allocateStatPointSuccess, (state, { stats }) => ({
    ...state,
    player: state.player ? { ...state.player, stats } : null,
    loading: false
  })),
  on(PlayerActions.allocateStatPointFailure, (state, { error }) => ({
    ...state,
    error,
    loading: false
  }))
);
