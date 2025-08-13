import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { QuestsService } from '../../core/services/quests.service';
import { PlayerService } from '../../core/services/player.service';
import { QuestDto, QuestCompleteResult } from '../../shared/models/quest.models';
import { PlayerProfileDto } from '../../shared/models/player.models';

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
      <div class="flex justify-between items-center mb-6">
        <h1 class="text-3xl font-bold text-mafia-accent">Görevler</h1>
        <div class="text-right">
          <p class="text-lg font-semibold text-mafia-accent">Seviye {{ playerLevel }}</p>
          <p class="text-sm text-gray-400">Maksimum Seviye: 50</p>
        </div>
      </div>
      
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
              <p>XP: {{ getEstimatedXpForLevel(quest.difficulty, playerLevel) }} | Para: {{ getEstimatedMoney(quest.difficulty) }}</p>
              <p class="mt-2">Süre: {{ quest.durationMinutes }} dakika | Bekleme: {{ quest.cooldownMinutes }} dakika</p>
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
  playerLevel = 1;

  constructor(
    private questsService: QuestsService,
    private playerService: PlayerService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadPlayerProfile();
    this.loadQuests();
  }

  loadPlayerProfile(): void {
    this.playerService.getProfile().subscribe({
      next: (profile: PlayerProfileDto) => {
        this.playerLevel = profile.level;
      },
      error: (error) => {
        console.error('Player profile yüklenirken hata:', error);
      }
    });
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
          
          // Player level'ını güncelle
          if (result.newLevel) {
            this.playerLevel = result.newLevel;
          }
          
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

  getEstimatedXpForLevel(difficulty: string, playerLevel: number): number {
    const baseXp = this.getEstimatedXp(difficulty);
    const levelMultiplier = 1.0 + (playerLevel - 1) * 0.1; // Her level %10 artış
    return Math.floor(baseXp * levelMultiplier);
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
