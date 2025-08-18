import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { PlayerService } from '../../core/services/player.service';
import { XpProgressionService, XpProgressionData } from '../../core/services/xp-progression.service';
import { PlayerProfileDto, PlayerStatsDto } from '../../shared/models/player.models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile.component.html'
})
export class ProfileComponent implements OnInit {
  profile: PlayerProfileDto | null = null;
  stats: PlayerStatsDto | null = null;
  xpProgression: XpProgressionData | null = null;
  loading = true;

  constructor(
    private playerService: PlayerService,
    private xpProgressionService: XpProgressionService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadProfile();
    this.loadStats();
    this.loadXpProgression();
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

  private loadXpProgression(): void {
    this.xpProgressionService.getXpProgression().subscribe({
      next: (progression) => {
        this.xpProgression = progression;
      },
      error: (error) => {
        console.error('XP progression load error:', error);
      }
    });
  }

  // XP hesaplama metodları
  getXpToNextLevel(currentLevel: number): number {
    if (currentLevel >= 50) return 0; // Max level
    if (!this.xpProgression) return 0; // Fallback to hardcoded values
    
    const levelData = this.xpProgression.levelProgression.find(p => p.level === currentLevel);
    return levelData ? levelData.xpToNext : 0;
  }

  getXpPercentage(currentXp: number, currentLevel: number): number {
    if (currentLevel >= 50) return 100; // Max level
    if (!currentXp || currentXp < 0) return 0; // Handle undefined/null
    const xpToNext = this.getXpToNextLevel(currentLevel);
    if (xpToNext <= 0) return 100; // Prevent division by zero
    return Math.min(100, (currentXp / xpToNext) * 100);
  }

  // Karakter resmi seçimi
  getCharacterVariant(): number {
    if (!this.profile || !this.profile.id) return 1;
    // ID'ye göre karakter varyantı seç (1-4 arası)
    return (Number(this.profile.id) % 4) + 1;
  }

  getCharacterImage(): string {
    const variant = this.getCharacterVariant();
    return `/assets/images/characters/character-${variant}.svg`;
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }
}
