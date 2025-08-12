import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { map, mergeMap, catchError } from 'rxjs/operators';
import * as InventoryActions from './inventory.actions';

@Injectable()
export class InventoryEffects {
  constructor(private actions$: Actions) {}

  loadInventory$ = createEffect(() => this.actions$.pipe(
    ofType(InventoryActions.loadInventory),
    mergeMap(() => {
      // TODO: Replace with actual API call
      const mockItems = [
        {
          id: '1',
          item: {
            id: 'weapon_1',
            name: 'Colt .45',
            slot: 'main_weapon',
            rarity: 'rare' as const,
            baseWeaponDamage: 45,
            affixes: [
              { type: 'karizma', value: 5, isPercent: false },
              { type: 'critical_chance', value: 10, isPercent: true }
            ],
            tags: ['firearm', 'pistol'],
            imageUrl: '/assets/weapons/colt45.png'
          },
          isEquipped: true,
          rollData: { quality: 0.85 }
        },
        {
          id: '2',
          item: {
            id: 'armor_1',
            name: 'Pinstripe Suit',
            slot: 'body',
            rarity: 'epic' as const,
            affixes: [
              { type: 'karizma', value: 8, isPercent: false },
              { type: 'mitigation', value: 15, isPercent: true }
            ],
            tags: ['armor', 'suit'],
            imageUrl: '/assets/armor/pinstripe.png'
          },
          isEquipped: true,
          rollData: { quality: 0.92 }
        }
      ];
      
      return of(InventoryActions.loadInventorySuccess({ items: mockItems }));
    })
  ));

  equipItem$ = createEffect(() => this.actions$.pipe(
    ofType(InventoryActions.equipItem),
    mergeMap(({ itemId }) => {
      // TODO: Replace with actual API call
      return of(InventoryActions.equipItemSuccess({ items: [] }));
    })
  ));
}
