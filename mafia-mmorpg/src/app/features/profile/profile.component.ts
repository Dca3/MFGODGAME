import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PlayerService } from '../../core/services/player.service';
import { PlayerProfileDto, PlayerStatsDto } from '../../shared/models/player.models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen bg-mafia-primary p-8">
      <div class="max-w-4xl mx-auto">
        <h1 class="text-4xl font-bold text-mafia-gold mb-8">Profil</h1>
        
        <div *ngIf="profile" class="bg-mafia-secondary rounded-lg p-6 mb-6">
          <h2 class="text-2xl font-bold text-mafia-gold mb-4">Karakter Bilgileri</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <p class="text-gray-300"><span class="text-mafia-gold">ID:</span> {{ profile.id }}</p>
              <p class="text-gray-300"><span class="text-mafia-gold">Seviye:</span> {{ profile.level }}</p>
              <p class="text-gray-300"><span class="text-mafia-gold">Para:</span> {{ profile.money }}</p>
              <p class="text-gray-300"><span class="text-mafia-gold">İtibar:</span> {{ profile.reputation }}</p>
            </div>
            <div>
              <p class="text-gray-300"><span class="text-mafia-gold">Oluşturulma:</span> {{ profile.createdAt | date }}</p>
            </div>
          </div>
        </div>

        <div *ngIf="stats" class="bg-mafia-secondary rounded-lg p-6">
          <h2 class="text-2xl font-bold text-mafia-gold mb-4">Karakter İstatistikleri</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <p class="text-gray-300"><span class="text-mafia-gold">Karizma (K):</span> {{ stats.k }}</p>
              <p class="text-gray-300"><span class="text-mafia-gold">Güç (G):</span> {{ stats.g }}</p>
              <p class="text-gray-300"><span class="text-mafia-gold">Zeka (Z):</span> {{ stats.z }}</p>
              <p class="text-gray-300"><span class="text-mafia-gold">Hayat (H):</span> {{ stats.h }}</p>
            </div>
            <div>
              <p class="text-gray-300"><span class="text-mafia-gold">Kullanılabilir Puan:</span> {{ stats.freePoints }}</p>
            </div>
          </div>
        </div>

        <div *ngIf="loading" class="text-center text-mafia-gold">
          Yükleniyor...
        </div>
      </div>
    </div>
  `
})
export class ProfileComponent implements OnInit {
  profile: PlayerProfileDto | null = null;
  stats: PlayerStatsDto | null = null;
  loading = true;

  constructor(private playerService: PlayerService) {}

  ngOnInit(): void {
    this.loadProfile();
    this.loadStats();
  }

  private loadProfile(): void {
    this.playerService.getProfile().subscribe({
      next: (profile) => {
        this.profile = profile;
        this.loading = false;
      },
      error: (error) => {
        console.error('Profile load error:', error);
        this.loading = false;
      }
    });
  }

  private loadStats(): void {
    this.playerService.getStats().subscribe({
      next: (stats) => {
        this.stats = stats;
      },
      error: (error) => {
        console.error('Stats load error:', error);
      }
    });
  }
}
