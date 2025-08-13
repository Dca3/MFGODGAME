import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LeaderboardService } from '../../core/services/leaderboard.service';
import { LeaderboardEntryDto } from '../../shared/models/leaderboard.models';

@Component({
  selector: 'app-leaderboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen bg-mafia-primary p-8">
      <div class="max-w-6xl mx-auto">
        <h1 class="text-4xl font-bold text-mafia-gold mb-8">Lider Tablosu</h1>
        
        <div class="bg-mafia-secondary rounded-lg p-6">
          <h2 class="text-2xl font-bold text-mafia-gold mb-4">Global Sıralama</h2>
          
          <div *ngIf="leaderboard.length > 0" class="overflow-x-auto">
            <table class="w-full text-left">
              <thead>
                <tr class="border-b border-gray-600">
                  <th class="py-3 px-4 text-mafia-gold font-bold">Sıra</th>
                  <th class="py-3 px-4 text-mafia-gold font-bold">Oyuncu</th>
                  <th class="py-3 px-4 text-mafia-gold font-bold">MMR</th>
                  <th class="py-3 px-4 text-mafia-gold font-bold">Seviye</th>
                  <th class="py-3 px-4 text-mafia-gold font-bold">İtibar</th>
                </tr>
              </thead>
              <tbody>
                <tr 
                  *ngFor="let entry of leaderboard" 
                  class="border-b border-gray-700 hover:bg-gray-800"
                >
                  <td class="py-3 px-4 text-gray-300">{{ entry.rank }}</td>
                  <td class="py-3 px-4 text-gray-300">{{ entry.name || 'Bilinmeyen' }}</td>
                  <td class="py-3 px-4 text-gray-300">{{ entry.mmr }}</td>
                  <td class="py-3 px-4 text-gray-300">{{ entry.level || '-' }}</td>
                  <td class="py-3 px-4 text-gray-300">{{ entry.reputation || '-' }}</td>
                </tr>
              </tbody>
            </table>
          </div>

          <div *ngIf="leaderboard.length === 0 && !loading" class="text-center text-gray-300">
            <p class="text-xl">Henüz sıralama verisi yok</p>
          </div>

          <div *ngIf="loading" class="text-center text-mafia-gold">
            Yükleniyor...
          </div>
        </div>
      </div>
    </div>
  `
})
export class LeaderboardComponent implements OnInit {
  leaderboard: LeaderboardEntryDto[] = [];
  loading = true;

  constructor(private leaderboardService: LeaderboardService) {}

  ngOnInit(): void {
    this.loadGlobalLeaderboard();
  }

  private loadGlobalLeaderboard(): void {
    this.leaderboardService.getGlobalLeaderboard(100).subscribe({
      next: (entries) => {
        this.leaderboard = entries;
        this.loading = false;
      },
      error: (error) => {
        console.error('Leaderboard load error:', error);
        this.loading = false;
      }
    });
  }
}
