import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DuelHubService } from '../../core/services/duel-hub.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-duel',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen bg-mafia-primary p-8">
      <div class="max-w-4xl mx-auto">
        <h1 class="text-4xl font-bold text-mafia-gold mb-8">Düello</h1>
        
        <div class="bg-mafia-secondary rounded-lg p-6">
          <h2 class="text-2xl font-bold text-mafia-gold mb-4">Düello Durumu</h2>
          
          <div class="text-center text-gray-300">
            <p class="text-xl mb-4">Düello sistemi yakında aktif olacak</p>
            <p>Gerçek zamanlı PvP düelloları yapabileceksin</p>
          </div>

          <div class="mt-6 text-center">
            <button 
              (click)="backToLobby()" 
              class="btn-primary"
            >
              Lobby'ye Dön
            </button>
          </div>
        </div>
      </div>
    </div>
  `
})
export class DuelComponent implements OnInit, OnDestroy {
  private subscriptions: Subscription[] = [];

  constructor(
    private duelHubService: DuelHubService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // TODO: Implement duel logic
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  backToLobby(): void {
    this.router.navigate(['/lobby']);
  }
}
