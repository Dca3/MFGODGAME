import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { map, mergeMap, catchError } from 'rxjs/operators';
import * as QuestActions from './quest.actions';

@Injectable()
export class QuestEffects {
  constructor(private actions$: Actions) {}

  loadAvailableQuests$ = createEffect(() => this.actions$.pipe(
    ofType(QuestActions.loadAvailableQuests),
    mergeMap(() => {
      // TODO: Replace with actual API call
      const mockQuests = [
        {
          id: '1',
          title: 'Dumanlı Liman',
          description: 'Liman ambarında rakip ailenin sevkiyatını sabote et',
          difficulty: 'medium' as const,
          story: 'Don Corleone\'nin limanındaki rakip aile sevkiyatını durdurmamız gerekiyor. Zeka yüksekse keşif aşamasında düşman hasarı azalır.',
          rewards: [
            { type: 'money' as const, value: 5000 },
            { type: 'experience' as const, value: 250 },
            { type: 'reputation' as const, value: 10 }
          ],
          requirements: {
            level: 10,
            reputation: 50
          },
          location: 'Liman Bölgesi',
          npcName: 'Don Corleone'
        },
        {
          id: '2',
          title: 'Mürekkep ve Kan',
          description: 'Bir gazeteci dosyasını çal',
          difficulty: 'hard' as const,
          story: 'Gazeteci bizi tehdit ediyor. Karizma kontrolü ile ikna edip sessiz çözebilir, başaramazsa çatışma.',
          rewards: [
            { type: 'money' as const, value: 8000 },
            { type: 'experience' as const, value: 400 },
            { type: 'reputation' as const, value: 15 }
          ],
          requirements: {
            level: 15,
            stats: { karizma: 20 }
          },
          location: 'Şehir Merkezi',
          npcName: 'Don Vito'
        }
      ];
      
      return of(QuestActions.loadAvailableQuestsSuccess({ quests: mockQuests }));
    })
  ));

  loadPlayerQuests$ = createEffect(() => this.actions$.pipe(
    ofType(QuestActions.loadPlayerQuests),
    mergeMap(() => {
      // TODO: Replace with actual API call
      const mockPlayerQuests = [
        {
          id: '1',
          questId: '1',
          state: 'active' as const,
          progress: 75,
          startedAt: new Date('2024-01-15')
        }
      ];
      
      return of(QuestActions.loadPlayerQuestsSuccess({ playerQuests: mockPlayerQuests }));
    })
  ));
}
