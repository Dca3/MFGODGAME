export interface InventoryItemDto {
  id: string;
  itemDefinitionId: string;
  name: string;
  slot: string;
  rarity: string;
  itemLevel: number;
  requiredLevel: number;
  baseK?: number;
  baseG?: number;
  baseZ?: number;
  baseH?: number;
  isEquipped: boolean;
}

export interface EquipItemRequest {
  itemId: string;
}

export interface UnequipItemRequest {
  itemId: string;
}
