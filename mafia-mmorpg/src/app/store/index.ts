import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';

import { playerReducer } from './player/player.reducer';
import { inventoryReducer } from './inventory/inventory.reducer';
import { questReducer } from './quest/quest.reducer';
import { duelReducer } from './duel/duel.reducer';
import { leaderboardReducer } from './leaderboard/leaderboard.reducer';

// import { PlayerEffects } from './player/player.effects';
// import { InventoryEffects } from './inventory/inventory.effects';
// import { QuestEffects } from './quest/quest.effects';
// import { DuelEffects } from './duel/duel.effects';
// import { LeaderboardEffects } from './leaderboard/leaderboard.effects';

export const storeProviders = [
  provideStore({
    player: playerReducer,
    inventory: inventoryReducer,
    quest: questReducer,
    duel: duelReducer,
    leaderboard: leaderboardReducer
  }),
  // provideEffects([
  //   PlayerEffects,
  //   InventoryEffects,
  //   QuestEffects,
  //   DuelEffects,
  //   LeaderboardEffects
  // ]),
  provideStoreDevtools({
    maxAge: 25,
    logOnly: false
  })
];
