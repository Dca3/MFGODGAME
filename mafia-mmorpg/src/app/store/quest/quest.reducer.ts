import { createReducer, on } from '@ngrx/store';
import * as QuestActions from './quest.actions';

export interface QuestState {
  availableQuests: any[];
  playerQuests: any[];
  loading: boolean;
  error: string | null;
}

export const initialState: QuestState = {
  availableQuests: [],
  playerQuests: [],
  loading: false,
  error: null
};

export const questReducer = createReducer(
  initialState,
  on(QuestActions.loadAvailableQuests, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  on(QuestActions.loadAvailableQuestsSuccess, (state, { quests }) => ({
    ...state,
    availableQuests: quests,
    loading: false
  })),
  on(QuestActions.loadAvailableQuestsFailure, (state, { error }) => ({
    ...state,
    error,
    loading: false
  })),
  on(QuestActions.loadPlayerQuests, (state) => ({
    ...state,
    loading: true
  })),
  on(QuestActions.loadPlayerQuestsSuccess, (state, { playerQuests }) => ({
    ...state,
    playerQuests,
    loading: false
  })),
  on(QuestActions.loadPlayerQuestsFailure, (state, { error }) => ({
    ...state,
    error,
    loading: false
  }))
);
