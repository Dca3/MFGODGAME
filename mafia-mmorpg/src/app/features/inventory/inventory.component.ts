import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { InventoryService } from '../../core/services/inventory.service';
import { PlayerService } from '../../core/services/player.service';
import { InventoryItemDto } from '../../shared/models/inventory.models';
import { PlayerProfileDto } from '../../shared/models/player.models';

interface DragState {
  isDragging: boolean;
  draggedItem: InventoryItemDto | null;
  draggedSlot: string | null;
}

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './inventory.component.html',
  styleUrls: ['./inventory.component.css']
})
export class InventoryComponent implements OnInit {
  inventory: (InventoryItemDto & { equipping?: boolean; unequipping?: boolean })[] = [];
  profile: PlayerProfileDto | null = null;
  loading = true;
  dragState: DragState = {
    isDragging: false,
    draggedItem: null,
    draggedSlot: null
  };

  constructor(
    private inventoryService: InventoryService,
    private playerService: PlayerService,
    private snackBar: MatSnackBar,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadPlayerProfile();
    this.loadInventory();
  }

  loadPlayerProfile(): void {
    this.playerService.getProfile().subscribe({
      next: (profile: PlayerProfileDto) => {
        this.profile = profile;
      },
      error: (error) => {
        console.error('Player profile yüklenirken hata:', error);
      }
    });
  }

  loadInventory(): void {
    this.loading = true;
    this.inventoryService.getInventory().subscribe({
      next: (inventory) => {
        this.inventory = inventory.map(item => ({ ...item, equipping: false, unequipping: false }));
        this.loading = false;
      },
      error: (error) => {
        console.error('Envanter yüklenirken hata:', error);
        this.snackBar.open('Envanter yüklenirken hata oluştu', 'Kapat', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  equipItem(itemId: string): void {
    const item = this.inventory.find(i => i.itemDefinitionId === itemId);
    if (!item) return;

    item.equipping = true;
    this.inventoryService.equipItem(itemId).subscribe({
      next: () => {
        item.equipping = false;
        item.isEquipped = true;
        this.snackBar.open('Eşya takıldı!', 'Kapat', { duration: 2000 });
      },
      error: (error) => {
        item.equipping = false;
        console.error('Eşya takılırken hata:', error);
        this.snackBar.open('Eşya takılırken hata oluştu', 'Kapat', { duration: 3000 });
      }
    });
  }

  unequipItem(itemId: string): void {
    if (!itemId) return;
    
    const item = this.inventory.find(i => i.itemDefinitionId === itemId);
    if (!item) return;

    item.unequipping = true;
    this.inventoryService.unequipItem(itemId).subscribe({
      next: () => {
        item.unequipping = false;
        item.isEquipped = false;
        this.snackBar.open('Eşya çıkarıldı!', 'Kapat', { duration: 2000 });
      },
      error: (error) => {
        item.unequipping = false;
        console.error('Eşya çıkarılırken hata:', error);
        this.snackBar.open('Eşya çıkarılırken hata oluştu', 'Kapat', { duration: 3000 });
      }
    });
  }

  getSlotText(slot: string): string {
    const slotMap: { [key: string]: string } = {
      'Weapon': 'Silah',
      'Glasses': 'Gözlük',
      'Suit': 'Smokin',
      'Accessory': 'Aksesuar'
    };
    return slotMap[slot] || slot;
  }

  getRarityText(rarity: string): string {
    const rarityMap: { [key: string]: string } = {
      'Common': 'Yaygın',
      'Uncommon': 'Nadir',
      'Rare': 'Ender',
      'Epic': 'Efsanevi',
      'Legendary': 'Destansı'
    };
    return rarityMap[rarity] || rarity;
  }

  getRarityClass(rarity: string): string {
    const classMap: { [key: string]: string } = {
      'Common': 'bg-gray-600 text-white',
      'Uncommon': 'bg-green-600 text-white',
      'Rare': 'bg-blue-600 text-white',
      'Epic': 'bg-purple-600 text-white',
      'Legendary': 'bg-yellow-600 text-black'
    };
    return classMap[rarity] || 'bg-gray-600 text-white';
  }

  // Yeni fonksiyonlar
  getEquippedItem(slot: string): InventoryItemDto | undefined {
    return this.inventory.find(item => item.isEquipped && item.slot === slot);
  }

  getItemImage(slot: string, rarity: string): string {
    // Slot ve rarity'ye göre item resimleri
    const slotMap: { [key: string]: { [key: string]: string } } = {
      'Weapon': {
        'Common': '/assets/images/items/weapon-common.svg',
        'Uncommon': '/assets/images/items/weapon-uncommon.svg',
        'Rare': '/assets/images/items/weapon-rare.svg',
        'Epic': '/assets/images/items/weapon-epic.svg',
        'Legendary': '/assets/images/items/weapon-legendary.svg'
      },
      'Head': {
        'Common': '/assets/images/items/head-common.svg',
        'Uncommon': '/assets/images/items/head-common.svg', // Fallback
        'Rare': '/assets/images/items/head-common.svg', // Fallback
        'Epic': '/assets/images/items/head-common.svg', // Fallback
        'Legendary': '/assets/images/items/head-legendary.svg'
      },
      'Chest': {
        'Common': '/assets/images/items/chest-common.svg',
        'Uncommon': '/assets/images/items/chest-common.svg', // Fallback
        'Rare': '/assets/images/items/chest-common.svg', // Fallback
        'Epic': '/assets/images/items/chest-common.svg', // Fallback
        'Legendary': '/assets/images/items/chest-legendary.svg'
      },
      'Hands': {
        'Common': '/assets/images/items/hands-common.svg',
        'Uncommon': '/assets/images/items/hands-common.svg', // Fallback
        'Rare': '/assets/images/items/hands-common.svg', // Fallback
        'Epic': '/assets/images/items/hands-common.svg', // Fallback
        'Legendary': '/assets/images/items/hands-common.svg' // Fallback
      },
      'Legs': {
        'Common': '/assets/images/items/legs-common.svg',
        'Uncommon': '/assets/images/items/legs-common.svg', // Fallback
        'Rare': '/assets/images/items/legs-common.svg', // Fallback
        'Epic': '/assets/images/items/legs-common.svg', // Fallback
        'Legendary': '/assets/images/items/legs-common.svg' // Fallback
      },
      'Feet': {
        'Common': '/assets/images/items/feet-common.svg',
        'Uncommon': '/assets/images/items/feet-common.svg', // Fallback
        'Rare': '/assets/images/items/feet-common.svg', // Fallback
        'Epic': '/assets/images/items/feet-common.svg', // Fallback
        'Legendary': '/assets/images/items/feet-common.svg' // Fallback
      }
    };
    
    return slotMap[slot]?.[rarity] || slotMap['Weapon']?.[rarity] || '/assets/images/items/weapon-common.svg';
  }

  onItemClick(item: InventoryItemDto): void {
    // Item'a tıklandığında tooltip gösterilir (CSS ile)
    console.log('Item clicked:', item.name);
  }

  // Sürükle-bırak işlevleri
  onDragStart(event: DragEvent, item: InventoryItemDto): void {
    if (!event.dataTransfer) return;
    
    this.dragState.isDragging = true;
    this.dragState.draggedItem = item;
    
    // Drag görselini ayarla
    const img = new Image();
    img.src = this.getItemImage(item.slot || 'Weapon', item.rarity || 'Common');
    event.dataTransfer.setDragImage(img, 25, 25);
    event.dataTransfer.effectAllowed = 'move';
    
    // Item verilerini transfer et
    event.dataTransfer.setData('application/json', JSON.stringify({
      itemId: item.itemDefinitionId,
      slot: item.slot,
      rarity: item.rarity
    }));
  }

  onDragEnd(event: DragEvent): void {
    this.dragState.isDragging = false;
    this.dragState.draggedItem = null;
    this.dragState.draggedSlot = null;
  }

  onDragOver(event: DragEvent, slot: string): void {
    event.preventDefault();
    event.dataTransfer!.dropEffect = 'move';
    this.dragState.draggedSlot = slot;
  }

  onDragLeave(event: DragEvent): void {
    this.dragState.draggedSlot = null;
  }

  onDrop(event: DragEvent, targetSlot: string): void {
    event.preventDefault();
    
    if (!this.dragState.draggedItem) return;
    
    const draggedItem = this.dragState.draggedItem;
    
    // Slot uyumluluğunu kontrol et
    if (draggedItem.slot !== targetSlot) {
      this.snackBar.open(`${draggedItem.name} bu slot'a takılamaz!`, 'Kapat', { duration: 2000 });
      return;
    }
    
    // Seviye kontrolü
    if (draggedItem.requiredLevel > (this.profile?.level || 1)) {
      this.snackBar.open(`Bu eşyayı takmak için seviye ${draggedItem.requiredLevel} gerekli!`, 'Kapat', { duration: 2000 });
      return;
    }
    
    // Eşyayı tak
    this.equipItem(draggedItem.itemDefinitionId);
    
    this.dragState.isDragging = false;
    this.dragState.draggedItem = null;
    this.dragState.draggedSlot = null;
  }

  // Slot'a item bırakma işlevi
  onSlotDrop(event: DragEvent, slot: string): void {
    event.preventDefault();
    
    try {
      const data = JSON.parse(event.dataTransfer!.getData('application/json'));
      const item = this.inventory.find(i => i.itemDefinitionId === data.itemId);
      
      if (!item) return;
      
      // Slot uyumluluğunu kontrol et
      if (item.slot !== slot) {
        this.snackBar.open(`${item.name} bu slot'a takılamaz!`, 'Kapat', { duration: 2000 });
        return;
      }
      
      // Seviye kontrolü
      if (item.requiredLevel > (this.profile?.level || 1)) {
        this.snackBar.open(`Bu eşyayı takmak için seviye ${item.requiredLevel} gerekli!`, 'Kapat', { duration: 2000 });
        return;
      }
      
      // Eşyayı tak
      this.equipItem(item.itemDefinitionId);
      
    } catch (error) {
      console.error('Drop data parse error:', error);
    }
  }

  // Slot'tan item çıkarma işlevi
  onSlotClick(slot: string): void {
    const equippedItem = this.getEquippedItem(slot);
    if (equippedItem) {
      this.unequipItem(equippedItem.itemDefinitionId);
    }
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }
}
