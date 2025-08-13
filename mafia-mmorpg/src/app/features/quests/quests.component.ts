import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { QuestsService } from '../../core/services/quests.service';
import { QuestDto, QuestCompleteResult } from '../../shared/models/quest.models';

@Component({
  selector: 'app-quests',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatChipsModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="container mx-auto p-6">
      <h1 class="text-3xl font-bold mb-6 text-mafia-accent">Görevler</h1>
      
      <div *ngIf="loading" class="flex justify-center">
        <mat-spinner></mat-spinner>
      </div>

      <div *ngIf="!loading && quests.length === 0" class="text-center py-8">
        <p class="text-gray-400">Henüz görev bulunmuyor.</p>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <mat-card *ngFor="let quest of quests" class="quest-card">
          <mat-card-header>
            <mat-card-title class="text-lg font-semibold">{{ quest.title }}</mat-card-title>
            <mat-card-subtitle>{{ quest.npcName }} - {{ quest.location }}</mat-card-subtitle>
          </mat-card-header>
          
          <mat-card-content class="mt-4">
            <p class="text-gray-300 mb-4">{{ quest.description }}</p>
            
            <div class="flex flex-wrap gap-2 mb-4">
              <mat-chip [class]="getDifficultyClass(quest.difficulty)">
                {{ getDifficultyText(quest.difficulty) }}
              </mat-chip>
              <mat-chip class="bg-blue-600 text-white">
                Seviye {{ quest.requiredLevel }}
              </mat-chip>
            </div>

            <div class="text-sm text-gray-400">
              <p>Tahmini Ödüller:</p>
              <p>XP: {{ getEstimatedXp(quest.difficulty) }} | Para: {{ getEstimatedMoney(quest.difficulty) }}</p>
            </div>
          </mat-card-content>

          <mat-card-actions class="flex gap-2">
            <button 
              mat-raised-button 
              color="primary"
              (click)="startQuest(quest.id)"
              [disabled]="quest.started"
              class="flex-1">
              {{ quest.started ? 'Başlatıldı' : 'Başlat' }}
            </button>
            
            <button 
              *ngIf="quest.started"
              mat-raised-button 
              color="accent"
              (click)="completeQuest(quest.id)"
              [disabled]="quest.completing"
              class="flex-1">
              {{ quest.completing ? 'Tamamlanıyor...' : 'Tamamla' }}
            </button>
          </mat-card-actions>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .quest-card {
      @apply bg-mafia-secondary border border-gray-700;
    }
    
    .quest-card:hover {
      @apply border-mafia-accent transition-colors duration-200;
    }
  `]
})
export class QuestsComponent implements OnInit {
  quests: (QuestDto & { started?: boolean; completing?: boolean })[] = [];
  loading = true;

  constructor(
    private questsService: QuestsService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadQuests();
  }

  loadQuests(): void {
    this.loading = true;
    this.questsService.getAvailable().subscribe({
      next: (quests) => {
        this.quests = quests.map(q => ({ ...q, started: false, completing: false }));
        this.loading = false;
      },
      error: (error) => {
        console.error('Görevler yüklenirken hata:', error);
        this.snackBar.open('Görevler yüklenirken hata oluştu', 'Kapat', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  startQuest(questId: string): void {
    const quest = this.quests.find(q => q.id === questId);
    if (!quest) return;

    this.questsService.start(questId).subscribe({
      next: () => {
        quest.started = true;
        this.snackBar.open('Görev başlatıldı!', 'Kapat', { duration: 2000 });
      },
      error: (error) => {
        console.error('Görev başlatılırken hata:', error);
        this.snackBar.open('Görev başlatılırken hata oluştu', 'Kapat', { duration: 3000 });
      }
    });
  }

  completeQuest(questId: string): void {
    const quest = this.quests.find(q => q.id === questId);
    if (!quest) return;

    quest.completing = true;
    this.questsService.complete(questId).subscribe({
      next: (result: QuestCompleteResult) => {
        quest.completing = false;
        
        if (result.success) {
          let message = `Görev tamamlandı! +${result.gainedXp} XP, +${result.money} Para`;
          
          if (result.newLevel) {
            message += `, Seviye ${result.newLevel}!`;
          }
          
          if (result.freePointsGained) {
            message += `, +${result.freePointsGained} Serbest Puan`;
          }
          
          if (result.rewardItemIds.length > 0) {
            message += `, ${result.rewardItemIds.length} Eşya`;
          }
          
          this.snackBar.open(message, 'Kapat', { duration: 5000 });
          
          // Görevi listeden kaldır
          this.quests = this.quests.filter(q => q.id !== questId);
        } else {
          this.snackBar.open('Görev başarısız oldu!', 'Kapat', { duration: 3000 });
        }
      },
      error: (error) => {
        quest.completing = false;
        console.error('Görev tamamlanırken hata:', error);
        this.snackBar.open('Görev tamamlanırken hata oluştu', 'Kapat', { duration: 3000 });
      }
    });
  }

  getDifficultyClass(difficulty: string): string {
    switch (difficulty) {
      case 'Easy': return 'bg-green-600 text-white';
      case 'Normal': return 'bg-blue-600 text-white';
      case 'Hard': return 'bg-orange-600 text-white';
      case 'Mythic': return 'bg-red-600 text-white';
      default: return 'bg-gray-600 text-white';
    }
  }

  getDifficultyText(difficulty: string): string {
    switch (difficulty) {
      case 'Easy': return 'Kolay';
      case 'Normal': return 'Normal';
      case 'Hard': return 'Zor';
      case 'Mythic': return 'Efsanevi';
      default: return difficulty;
    }
  }

  getEstimatedXp(difficulty: string): number {
    switch (difficulty) {
      case 'Easy': return 120;
      case 'Normal': return 220;
      case 'Hard': return 380;
      case 'Mythic': return 650;
      default: return 100;
    }
  }

  getEstimatedMoney(difficulty: string): number {
    switch (difficulty) {
      case 'Easy': return 150;
      case 'Normal': return 300;
      case 'Hard': return 600;
      case 'Mythic': return 1200;
      default: return 100;
    }
  }
}
