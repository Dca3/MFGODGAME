import { createReducer, on } from '@ngrx/store';
import * as InventoryActions from './inventory.actions';

export interface InventoryState {
  items: any[];
  loading: boolean;
  error: string | null;
}

export const initialState: InventoryState = {
  items: [],
  loading: false,
  error: null
};

export const inventoryReducer = createReducer(
  initialState,
  on(InventoryActions.loadInventory, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  on(InventoryActions.loadInventorySuccess, (state, { items }) => ({
    ...state,
    items,
    loading: false
  })),
  on(InventoryActions.loadInventoryFailure, (state, { error }) => ({
    ...state,
    error,
    loading: false
  })),
  on(InventoryActions.equipItem, (state) => ({
    ...state,
    loading: true
  })),
  on(InventoryActions.equipItemSuccess, (state, { items }) => ({
    ...state,
    items,
    loading: false
  })),
  on(InventoryActions.equipItemFailure, (state, { error }) => ({
    ...state,
    error,
    loading: false
  }))
);
