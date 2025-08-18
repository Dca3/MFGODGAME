import { Component, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './core/services/auth.service';
import { PlayerService } from './core/services/player.service';
import { PlayerProfileDto } from './shared/models/player.models';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class App implements OnInit {
  title = 'Mafia MMORPG';
  isAuthenticated = false;
  loading = false;
  userProfile: PlayerProfileDto | null = null;

  constructor(
    private authService: AuthService,
    private playerService: PlayerService
  ) {}

  ngOnInit(): void {
    this.checkAuthStatus();
  }

  checkAuthStatus(): void {
    this.isAuthenticated = this.authService.isAuthenticated();
    if (this.isAuthenticated) {
      this.loadUserProfile();
    }
  }

  loadUserProfile(): void {
    this.playerService.getProfile().subscribe({
      next: (profile) => {
        this.userProfile = profile;
      },
      error: (error) => {
        console.error('Profile load error:', error);
      }
    });
  }

  getUserAvatar(): string {
    // Farklı yolları dene
    return 'assets/images/characters/character-1.svg';
  }

  logout(): void {
    this.authService.logout();
    this.isAuthenticated = false;
    this.userProfile = null;
  }
}
