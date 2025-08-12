import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { map, mergeMap, catchError } from 'rxjs/operators';
import * as DuelActions from './duel.actions';

@Injectable()
export class DuelEffects {
  constructor(private actions$: Actions) {}

  loadDuelHistory$ = createEffect(() => this.actions$.pipe(
    ofType(DuelActions.loadDuelHistory),
    mergeMap(() => {
      // TODO: Replace with actual API call
      const mockDuels = [
        {
          id: '1',
          player1Id: 'current',
          player2Id: 'opponent1',
          currentTurn: 10,
          maxTurns: 10,
          player1Hp: 0,
          player2Hp: 45,
          player1MaxHp: 100,
          player2MaxHp: 100,
          actions: [],
          status: 'finished' as const,
          winner: 'opponent1',
          log: ['Duel başladı', 'Player1 saldırdı', 'Player2 savundu', 'Player2 kazandı']
        }
      ];
      
      return of(DuelActions.loadDuelHistorySuccess({ duels: mockDuels }));
    })
  ));
}
