import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { PlayerService } from '../../core/services/player.service';
import { PlayerProfileDto, PlayerStatsDto, AllocateStatsRequest } from '../../shared/models/player.models';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, MatSnackBarModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  profile: PlayerProfileDto | null = null;
  stats: PlayerStatsDto | null = null;
  loading = true;
  
  // Stat allocation tracking
  tempStats: { [key: string]: number } = {};
  originalStats: { [key: string]: number } = {};

  constructor(
    private playerService: PlayerService,
    private snackBar: MatSnackBar,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.loading = true;
    
    // Load profile
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

    // Load stats
    this.playerService.getStats().subscribe({
      next: (stats) => {
        this.stats = stats;
        this.initializeTempStats();
      },
      error: (error) => {
        console.error('Stats load error:', error);
      }
    });
  }

  getCharacterImage(): string {
    // Farklı yolları dene
    return 'assets/images/characters/character-1.svg';
  }

  getXpPercentage(): number {
    if (!this.profile || !this.profile.experience) return 0;
    // Simplified XP calculation
    const currentLevel = this.profile.level;
    const xpToNext = currentLevel * 100; // Simple formula
    return Math.min(100, (this.profile.experience / xpToNext) * 100);
  }

  // Stat allocation methods
  initializeTempStats(): void {
    if (!this.stats) return;
    
    this.tempStats = {
      Karizma: this.stats.Karizma,
      Guc: this.stats.Guc,
      Zeka: this.stats.Zeka,
      Hayat: this.stats.Hayat
    };
    
    this.originalStats = { ...this.tempStats };
  }

  getStatDisplayName(stat: string): string {
    const names: { [key: string]: string } = {
      'Karizma': 'Karizma',
      'Guc': 'Güç',
      'Zeka': 'Zeka',
      'Hayat': 'Hayat'
    };
    return names[stat] || stat;
  }

  getCurrentStatValue(stat: string): number {
    return this.tempStats[stat] || 0;
  }

  canIncreaseStat(stat: string): boolean {
    if (!this.stats) return false;
    const currentValue = this.tempStats[stat] || 0;
    const originalValue = this.originalStats[stat] || 0;
    const usedPoints = this.getUsedPoints();
    return currentValue < 20 && usedPoints < this.stats.FreePoints;
  }

  canDecreaseStat(stat: string): boolean {
    const currentValue = this.tempStats[stat] || 0;
    const originalValue = this.originalStats[stat] || 0;
    return currentValue > originalValue;
  }

  increaseStat(stat: string): void {
    if (this.canIncreaseStat(stat)) {
      this.tempStats[stat] = (this.tempStats[stat] || 0) + 1;
    }
  }

  decreaseStat(stat: string): void {
    if (this.canDecreaseStat(stat)) {
      this.tempStats[stat] = (this.tempStats[stat] || 0) - 1;
    }
  }

  getUsedPoints(): number {
    let used = 0;
    Object.keys(this.tempStats).forEach(stat => {
      const current = this.tempStats[stat] || 0;
      const original = this.originalStats[stat] || 0;
      used += Math.max(0, current - original);
    });
    return used;
  }

  hasStatChanges(): boolean {
    return Object.keys(this.tempStats).some(stat => {
      const current = this.tempStats[stat] || 0;
      const original = this.originalStats[stat] || 0;
      return current !== original;
    });
  }

  applyStatChanges(): void {
    if (!this.stats || !this.hasStatChanges()) return;

    const request: AllocateStatsRequest = {
      Karizma: this.tempStats['Karizma'],
      Guc: this.tempStats['Guc'],
      Zeka: this.tempStats['Zeka'],
      Hayat: this.tempStats['Hayat']
    };

    this.playerService.allocateStats(request).subscribe({
      next: () => {
        this.snackBar.open('Stat puanları başarıyla dağıtıldı!', 'Kapat', { duration: 3000 });
        this.loadDashboardData(); // Reload data
      },
      error: (error) => {
        console.error('Stat allocation error:', error);
        this.snackBar.open('Stat puanları dağıtılırken hata oluştu!', 'Kapat', { duration: 3000 });
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }
}
