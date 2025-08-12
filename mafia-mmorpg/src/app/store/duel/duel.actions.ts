import { createAction, props } from '@ngrx/store';

export interface DuelAction {
  type: 'attack' | 'defend' | 'special';
  target?: string;
  data?: any;
}

export interface DuelState {
  id: string;
  player1Id: string;
  player2Id: string;
  currentTurn: number;
  maxTurns: number;
  player1Hp: number;
  player2Hp: number;
  player1MaxHp: number;
  player2MaxHp: number;
  actions: DuelAction[];
  status: 'waiting' | 'active' | 'finished';
  winner?: string;
  log: string[];
}

// Queue Management
export const joinQueue = createAction('[Duel] Join Queue');
export const leaveQueue = createAction('[Duel] Leave Queue');
export const matchFound = createAction(
  '[Duel] Match Found',
  props<{ duelId: string; opponent: string }>()
);

// Duel Actions
export const submitAction = createAction(
  '[Duel] Submit Action',
  props<{ action: DuelAction }>()
);
export const duelStateUpdated = createAction(
  '[Duel] State Updated',
  props<{ state: DuelState }>()
);
export const duelEnded = createAction(
  '[Duel] Duel Ended',
  props<{ winner: string; mmrChange: number }>()
);

// Load Duel History
export const loadDuelHistory = createAction('[Duel] Load History');
export const loadDuelHistorySuccess = createAction(
  '[Duel] Load History Success',
  props<{ duels: DuelState[] }>()
);
export const loadDuelHistoryFailure = createAction(
  '[Duel] Load History Failure',
  props<{ error: string }>()
);
