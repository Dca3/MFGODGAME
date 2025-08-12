import { createAction, props } from '@ngrx/store';

export interface QuestReward {
  type: 'money' | 'experience' | 'reputation' | 'item';
  value: number;
  itemId?: string;
}

export interface Quest {
  id: string;
  title: string;
  description: string;
  difficulty: 'easy' | 'medium' | 'hard' | 'legendary';
  story: string;
  rewards: QuestReward[];
  requirements: {
    level?: number;
    reputation?: number;
    stats?: Record<string, number>;
  };
  location: string;
  npcName: string;
}

export interface PlayerQuest {
  id: string;
  questId: string;
  state: 'available' | 'active' | 'completed' | 'failed';
  progress: number;
  startedAt?: Date;
  completedAt?: Date;
}

// Load Quests
export const loadAvailableQuests = createAction('[Quest] Load Available Quests');
export const loadAvailableQuestsSuccess = createAction(
  '[Quest] Load Available Quests Success',
  props<{ quests: Quest[] }>()
);
export const loadAvailableQuestsFailure = createAction(
  '[Quest] Load Available Quests Failure',
  props<{ error: string }>()
);

export const loadPlayerQuests = createAction('[Quest] Load Player Quests');
export const loadPlayerQuestsSuccess = createAction(
  '[Quest] Load Player Quests Success',
  props<{ playerQuests: PlayerQuest[] }>()
);
export const loadPlayerQuestsFailure = createAction(
  '[Quest] Load Player Quests Failure',
  props<{ error: string }>()
);

// Start Quest
export const startQuest = createAction(
  '[Quest] Start Quest',
  props<{ questId: string }>()
);
export const startQuestSuccess = createAction(
  '[Quest] Start Quest Success',
  props<{ playerQuest: PlayerQuest }>()
);
export const startQuestFailure = createAction(
  '[Quest] Start Quest Failure',
  props<{ error: string }>()
);

// Complete Quest
export const completeQuest = createAction(
  '[Quest] Complete Quest',
  props<{ questId: string }>()
);
export const completeQuestSuccess = createAction(
  '[Quest] Complete Quest Success',
  props<{ playerQuest: PlayerQuest; rewards: QuestReward[] }>()
);
export const completeQuestFailure = createAction(
  '[Quest] Complete Quest Failure',
  props<{ error: string }>()
);
