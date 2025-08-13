import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { LobbyComponent } from './features/lobby/lobby.component';
import { ProfileComponent } from './features/profile/profile.component';
import { InventoryComponent } from './features/inventory/inventory.component';
import { QuestsComponent } from './features/quests/quests.component';
import { LeaderboardComponent } from './features/leaderboard/leaderboard.component';
import { DuelComponent } from './features/duel/duel.component';

export const routes: Routes = [
  { path: '', redirectTo: '/lobby', pathMatch: 'full' },
  { path: 'auth/login', component: LoginComponent },
  { path: 'auth/register', component: RegisterComponent },
  { 
    path: 'lobby', 
    component: LobbyComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'me', 
    component: ProfileComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'inventory', 
    component: InventoryComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'quests', 
    component: QuestsComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'leaderboard', 
    component: LeaderboardComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'duel', 
    component: DuelComponent,
    canActivate: [AuthGuard]
  }
];
