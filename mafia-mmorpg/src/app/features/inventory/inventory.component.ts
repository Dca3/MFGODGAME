import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InventoryService } from '../../core/services/inventory.service';
import { InventoryItemDto } from '../../shared/models/inventory.models';

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen bg-mafia-primary p-8">
      <div class="max-w-6xl mx-auto">
        <h1 class="text-4xl font-bold text-mafia-gold mb-8">Envanter</h1>
        
        <div *ngIf="items.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <div 
            *ngFor="let item of items" 
            class="bg-mafia-secondary rounded-lg p-4 border-2"
            [class.border-mafia-gold]="item.isEquipped"
            [class.border-gray-600]="!item.isEquipped"
          >
            <h3 class="text-lg font-bold text-mafia-gold mb-2">{{ item.name }}</h3>
            <div class="space-y-2 text-sm text-gray-300">
              <p><span class="text-mafia-gold">Slot:</span> {{ item.slot }}</p>
              <p><span class="text-mafia-gold">Nadir:</span> {{ item.rarity }}</p>
              <p *ngIf="item.isEquipped" class="text-green-400 font-bold">Giyili</p>
            </div>
            <div class="mt-4">
              <button 
                *ngIf="!item.isEquipped"
                (click)="equipItem(item.id)" 
                [disabled]="loading"
                class="btn-primary w-full"
              >
                {{ loading ? 'Giyiliyor...' : 'Giy' }}
              </button>
              <button 
                *ngIf="item.isEquipped"
                disabled
                class="bg-gray-600 text-gray-400 font-bold py-2 px-4 rounded w-full cursor-not-allowed"
              >
                Giyili
              </button>
            </div>
          </div>
        </div>

        <div *ngIf="items.length === 0 && !loading" class="text-center text-gray-300">
          <p class="text-xl">Envanterin boş</p>
          <p>Görevlerden veya düellolardan eşya kazanabilirsin</p>
        </div>

        <div *ngIf="loading" class="text-center text-mafia-gold">
          Yükleniyor...
        </div>
      </div>
    </div>
  `
})
export class InventoryComponent implements OnInit {
  items: InventoryItemDto[] = [];
  loading = true;

  constructor(private inventoryService: InventoryService) {}

  ngOnInit(): void {
    this.loadInventory();
  }

  private loadInventory(): void {
    this.inventoryService.getInventory().subscribe({
      next: (items) => {
        this.items = items;
        this.loading = false;
      },
      error: (error) => {
        console.error('Inventory load error:', error);
        this.loading = false;
      }
    });
  }

  equipItem(itemId: string): void {
    this.loading = true;
    this.inventoryService.equipItem(itemId).subscribe({
      next: () => {
        this.loadInventory(); // Refresh inventory
      },
      error: (error) => {
        console.error('Equip error:', error);
        this.loading = false;
      }
    });
  }
}
