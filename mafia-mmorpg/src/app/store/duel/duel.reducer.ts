import { createReducer, on } from '@ngrx/store';
import * as DuelActions from './duel.actions';

export interface DuelStoreState {
  currentDuel: any | null;
  duelHistory: any[];
  inQueue: boolean;
  loading: boolean;
  error: string | null;
}

export const initialState: DuelStoreState = {
  currentDuel: null,
  duelHistory: [],
  inQueue: false,
  loading: false,
  error: null
};

export const duelReducer = createReducer(
  initialState,
  on(DuelActions.joinQueue, (state) => ({
    ...state,
    inQueue: true,
    loading: true
  })),
  on(DuelActions.leaveQueue, (state) => ({
    ...state,
    inQueue: false,
    loading: false
  })),
  on(DuelActions.matchFound, (state, { duelId, opponent }) => ({
    ...state,
    currentDuel: {
      id: duelId,
      player1Id: 'current',
      player2Id: opponent,
      currentTurn: 1,
      maxTurns: 10,
      player1Hp: 100,
      player2Hp: 100,
      player1MaxHp: 100,
      player2MaxHp: 100,
      actions: [],
      status: 'waiting' as const,
      log: []
    },
    inQueue: false,
    loading: false
  })),
  on(DuelActions.duelStateUpdated, (state, { state: duelState }) => ({
    ...state,
    currentDuel: duelState
  })),
  on(DuelActions.duelEnded, (state, { winner }) => ({
    ...state,
    currentDuel: state.currentDuel ? { ...state.currentDuel, status: 'finished' as const, winner } : null,
    loading: false
  })),
  on(DuelActions.loadDuelHistory, (state) => ({
    ...state,
    loading: true
  })),
  on(DuelActions.loadDuelHistorySuccess, (state, { duels }) => ({
    ...state,
    duelHistory: duels,
    loading: false
  })),
  on(DuelActions.loadDuelHistoryFailure, (state, { error }) => ({
    ...state,
    error,
    loading: false
  }))
);
