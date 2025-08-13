export interface QuestDto {
  id: string;
  title: string;
  description: string;
  difficulty: 'Easy' | 'Normal' | 'Hard' | 'Mythic';
  requiredLevel: number;
  location: string;
  npcName: string;
}

export interface QuestCompleteResult {
  success: boolean;
  gainedXp: number;
  money: number;
  rewardItemIds: string[];
  newLevel?: number;
  freePointsGained?: number;
}

export interface QuestReward {
  type: string;
  value: number;
}

export interface QuestRequirements {
  level?: number;
  reputation?: number;
  stats?: Record<string, number>;
}
