import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { map, mergeMap, catchError } from 'rxjs/operators';
import * as PlayerActions from './player.actions';

@Injectable()
export class PlayerEffects {
  constructor(private actions$: Actions) {}

  loadPlayer$ = createEffect(() => this.actions$.pipe(
    ofType(PlayerActions.loadPlayer),
    mergeMap(() => {
      // TODO: Replace with actual API call
      const mockPlayer = {
        id: '1',
        username: 'DonCorleone',
        level: 15,
        experience: 2500,
        money: 50000,
        reputation: 100,
        stats: {
          karizma: 25,
          guc: 18,
          zeka: 22,
          hayat: 20,
          freePoints: 5
        },
        mmr: 1250,
        wins: 45,
        losses: 12
      };
      
      return of(PlayerActions.loadPlayerSuccess({ player: mockPlayer }));
    })
  ));

  allocateStatPoint$ = createEffect(() => this.actions$.pipe(
    ofType(PlayerActions.allocateStatPoint),
    mergeMap(({ stat }) => {
      // TODO: Replace with actual API call
      const mockStats = {
        karizma: 25,
        guc: 18,
        zeka: 22,
        hayat: 20,
        freePoints: 4
      };
      
      // Simulate stat allocation
      if (mockStats.freePoints > 0) {
        mockStats[stat]++;
        mockStats.freePoints--;
      }
      
      return of(PlayerActions.allocateStatPointSuccess({ stats: mockStats }));
    })
  ));
}
