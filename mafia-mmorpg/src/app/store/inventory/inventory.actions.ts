import { createAction, props } from '@ngrx/store';

export interface ItemAffix {
  type: string;
  value: number;
  isPercent: boolean;
}

export interface Item {
  id: string;
  name: string;
  slot: string;
  rarity: 'common' | 'rare' | 'epic' | 'legendary';
  baseWeaponDamage?: number;
  affixes: ItemAffix[];
  tags: string[];
  imageUrl?: string;
}

export interface InventoryItem {
  id: string;
  item: Item;
  isEquipped: boolean;
  rollData: any;
}

// Load Inventory
export const loadInventory = createAction('[Inventory] Load Inventory');
export const loadInventorySuccess = createAction(
  '[Inventory] Load Inventory Success',
  props<{ items: InventoryItem[] }>()
);
export const loadInventoryFailure = createAction(
  '[Inventory] Load Inventory Failure',
  props<{ error: string }>()
);

// Equip/Unequip Item
export const equipItem = createAction(
  '[Inventory] Equip Item',
  props<{ itemId: string }>()
);
export const equipItemSuccess = createAction(
  '[Inventory] Equip Item Success',
  props<{ items: InventoryItem[] }>()
);
export const equipItemFailure = createAction(
  '[Inventory] Equip Item Failure',
  props<{ error: string }>()
);

export const unequipItem = createAction(
  '[Inventory] Unequip Item',
  props<{ itemId: string }>()
);
export const unequipItemSuccess = createAction(
  '[Inventory] Unequip Item Success',
  props<{ items: InventoryItem[] }>()
);
export const unequipItemFailure = createAction(
  '[Inventory] Unequip Item Failure',
  props<{ error: string }>()
);

// Craft/Upgrade
export const craftItem = createAction(
  '[Inventory] Craft Item',
  props<{ itemId: string; materials: string[] }>()
);
export const craftItemSuccess = createAction(
  '[Inventory] Craft Item Success',
  props<{ newItem: InventoryItem }>()
);
export const craftItemFailure = createAction(
  '[Inventory] Craft Item Failure',
  props<{ error: string }>()
);
