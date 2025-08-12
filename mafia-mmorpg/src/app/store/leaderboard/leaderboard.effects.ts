import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { map, mergeMap, catchError } from 'rxjs/operators';
import * as LeaderboardActions from './leaderboard.actions';

@Injectable()
export class LeaderboardEffects {
  constructor(private actions$: Actions) {}

  loadLeaderboard$ = createEffect(() => this.actions$.pipe(
    ofType(LeaderboardActions.loadLeaderboard),
    mergeMap(({ leaderboardType, region }) => {
      // TODO: Replace with actual API call
      const mockEntries = [
        {
          rank: 1,
          playerId: '1',
          username: 'DonCorleone',
          mmr: 1850,
          wins: 156,
          losses: 23,
          winRate: 87.2,
          level: 45
        },
        {
          rank: 2,
          playerId: '2',
          username: 'TonyMontana',
          mmr: 1820,
          wins: 142,
          losses: 31,
          winRate: 82.1,
          level: 42
        },
        {
          rank: 3,
          playerId: '3',
          username: 'VitoCorleone',
          mmr: 1790,
          wins: 134,
          losses: 28,
          winRate: 82.7,
          level: 40
        }
      ];
      
      return of(LeaderboardActions.loadLeaderboardSuccess({ 
        entries: mockEntries, 
        leaderboardType 
      }));
    })
  ));

  loadSeasons$ = createEffect(() => this.actions$.pipe(
    ofType(LeaderboardActions.loadSeasons),
    mergeMap(() => {
      // TODO: Replace with actual API call
      const mockSeasons = [
        {
          id: '1',
          name: 'Sezon 1: Başlangıç',
          startDate: new Date('2024-01-01'),
          endDate: new Date('2024-02-01'),
          status: 'ended' as const,
          rewards: {
            top1000: ['Bronze Smokin'],
            top100: ['Silver Smokin', 'Bronze Smokin'],
            top10: ['Gold Smokin', 'Silver Smokin', 'Bronze Smokin']
          }
        },
        {
          id: '2',
          name: 'Sezon 2: Yükseliş',
          startDate: new Date('2024-02-01'),
          endDate: new Date('2024-03-01'),
          status: 'active' as const,
          rewards: {
            top1000: ['Bronze Gözlük'],
            top100: ['Silver Gözlük', 'Bronze Gözlük'],
            top10: ['Gold Gözlük', 'Silver Gözlük', 'Bronze Gözlük']
          }
        }
      ];
      
      return of(LeaderboardActions.loadSeasonsSuccess({ seasons: mockSeasons }));
    })
  ));
}
