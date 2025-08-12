import { Component, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Store } from '@ngrx/store';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { Player } from './store/player/player.actions';
import { loadPlayer } from './store/player/player.actions';
import { loadAvailableQuests } from './store/quest/quest.actions';
import { loadInventory } from './store/inventory/inventory.actions';
import { loadLeaderboard } from './store/leaderboard/leaderboard.actions';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, CommonModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected readonly title = signal('Mafia MMORPG');
  
  player$: Observable<Player | null>;
  loading$: Observable<boolean>;

  constructor(private store: Store<any>) {
    this.player$ = this.store.select((state: any) => state.player.player);
    this.loading$ = this.store.select((state: any) => state.player.loading);
  }

  ngOnInit() {
    // Load initial data
    this.store.dispatch(loadPlayer());
    this.store.dispatch(loadAvailableQuests());
    this.store.dispatch(loadInventory());
    this.store.dispatch(loadLeaderboard({ leaderboardType: 'global' }));
  }
}
