import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DuelHubService } from '../../core/services/duel-hub.service';
import { AuthService } from '../../core/services/auth.service';
import { QueueState, MatchInfo } from '../../shared/models/pvp.models';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-lobby',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen bg-mafia-primary p-8">
      <div class="max-w-4xl mx-auto">
        <!-- Header -->
        <div class="flex justify-between items-center mb-8">
          <h1 class="text-4xl font-bold text-mafia-gold">Mafya Lobby</h1>
          <button 
            (click)="logout()" 
            class="btn-primary"
          >
            Çıkış Yap
          </button>
        </div>

        <!-- Queue Section -->
        <div class="bg-mafia-secondary rounded-lg p-6 mb-6">
          <h2 class="text-2xl font-bold text-mafia-gold mb-4">PvP Düello</h2>
          
          <div class="flex gap-4 mb-4">
            <button 
              *ngIf="queueState === 'idle'"
              (click)="joinQueue()" 
              [disabled]="loading"
              class="btn-primary"
            >
              {{ loading ? 'Kuyruğa Giriliyor...' : 'Kuyruğa Gir' }}
            </button>
            
            <button 
              *ngIf="queueState === 'queued'"
              (click)="leaveQueue()" 
              class="bg-gray-600 hover:bg-gray-700 text-white font-bold py-2 px-4 rounded transition-colors duration-200"
            >
              Kuyruktan Çık
            </button>
          </div>

          <!-- Queue Status -->
          <div class="text-gray-300">
            <p *ngIf="queueState === 'idle'">Düello için kuyruğa gir</p>
            <p *ngIf="queueState === 'queued'" class="text-yellow-400">Kuyrukta bekleniyor...</p>
            <p *ngIf="queueState === 'matched'" class="text-green-400">Eşleşme bulundu!</p>
            <p *ngIf="queueState === 'inDuel'" class="text-red-400">Düello devam ediyor...</p>
          </div>
        </div>

        <!-- Match Found Modal -->
        <div *ngIf="currentMatch && queueState === 'matched'" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div class="bg-mafia-secondary rounded-lg p-8 max-w-md w-full mx-4">
            <h3 class="text-2xl font-bold text-mafia-gold mb-4">Eşleşme Bulundu!</h3>
            <p class="text-gray-300 mb-6">
              Bir düello eşleşmesi bulundu. Kabul etmek istiyor musun?
            </p>
            <div class="flex gap-4">
              <button 
                (click)="acceptMatch()" 
                class="btn-primary flex-1"
              >
                Kabul Et
              </button>
              <button 
                (click)="declineMatch()" 
                class="bg-gray-600 hover:bg-gray-700 text-white font-bold py-2 px-4 rounded transition-colors duration-200 flex-1"
              >
                Reddet
              </button>
            </div>
          </div>
        </div>

        <!-- Navigation -->
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <div class="bg-mafia-secondary rounded-lg p-6">
            <h3 class="text-xl font-bold text-mafia-gold mb-4">Profil</h3>
            <p class="text-gray-300 mb-4">Karakter bilgilerini görüntüle</p>
            <button 
              (click)="navigateTo('/me')" 
              class="btn-primary w-full"
            >
              Profili Görüntüle
            </button>
          </div>

          <div class="bg-mafia-secondary rounded-lg p-6">
            <h3 class="text-xl font-bold text-mafia-gold mb-4">Envanter</h3>
            <p class="text-gray-300 mb-4">Eşyalarını yönet</p>
            <button 
              (click)="navigateTo('/inventory')" 
              class="btn-primary w-full"
            >
              Envanteri Aç
            </button>
          </div>

          <div class="bg-mafia-secondary rounded-lg p-6">
            <h3 class="text-xl font-bold text-mafia-gold mb-4">Görevler</h3>
            <p class="text-gray-300 mb-4">Mevcut görevleri görüntüle</p>
            <button 
              (click)="navigateTo('/quests')" 
              class="btn-primary w-full"
            >
              Görevleri Aç
            </button>
          </div>

          <div class="bg-mafia-secondary rounded-lg p-6">
            <h3 class="text-xl font-bold text-mafia-gold mb-4">Lider Tablosu</h3>
            <p class="text-gray-300 mb-4">En iyi oyuncuları gör</p>
            <button 
              (click)="navigateTo('/leaderboard')" 
              class="btn-primary w-full"
            >
              Lider Tablosu
            </button>
          </div>
        </div>
      </div>
    </div>
  `
})
export class LobbyComponent implements OnInit, OnDestroy {
  queueState: QueueState = 'idle';
  currentMatch: MatchInfo | null = null;
  loading = false;
  
  private subscriptions: Subscription[] = [];

  constructor(
    private duelHubService: DuelHubService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.startHub();
    this.subscribeToHubEvents();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    this.duelHubService.stop();
  }

  private async startHub(): Promise<void> {
    try {
      await this.duelHubService.start();
    } catch (error) {
      console.error('Failed to start hub:', error);
    }
  }

  private subscribeToHubEvents(): void {
    this.subscriptions.push(
      this.duelHubService.queueState$.subscribe(state => {
        this.queueState = state;
      }),
      
      this.duelHubService.match$.subscribe(match => {
        this.currentMatch = match;
      }),
      
      this.duelHubService.error$.subscribe(error => {
        if (error) {
          console.error('Hub error:', error);
          // TODO: Show error message to user
        }
      })
    );
  }

  async joinQueue(): Promise<void> {
    this.loading = true;
    try {
      await this.duelHubService.joinQueue();
    } catch (error) {
      console.error('Failed to join queue:', error);
    } finally {
      this.loading = false;
    }
  }

  async leaveQueue(): Promise<void> {
    try {
      await this.duelHubService.leaveQueue();
    } catch (error) {
      console.error('Failed to leave queue:', error);
    }
  }

  async acceptMatch(): Promise<void> {
    if (!this.currentMatch) return;
    
    try {
      await this.duelHubService.acceptMatch(this.currentMatch.matchId);
      this.router.navigate(['/duel']);
    } catch (error) {
      console.error('Failed to accept match:', error);
    }
  }

  declineMatch(): void {
    this.currentMatch = null;
    this.queueState = 'idle';
  }

  navigateTo(route: string): void {
    this.router.navigate([route]);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }
}
