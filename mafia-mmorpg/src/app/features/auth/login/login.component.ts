import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LoginRequest } from '../../../shared/models/auth.models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-mafia-primary">
      <div class="max-w-md w-full space-y-8 p-8 bg-mafia-secondary rounded-lg shadow-lg">
        <div class="text-center">
          <h2 class="text-3xl font-bold text-mafia-gold">Giriş Yap</h2>
          <p class="mt-2 text-gray-300">Mafya dünyasına hoş geldin</p>
        </div>
        
        <form class="mt-8 space-y-6" (ngSubmit)="onLogin()">
          <div>
            <label class="block text-sm font-medium text-gray-300">Email veya Kullanıcı Adı</label>
            <input 
              type="text" 
              [(ngModel)]="credentials.emailOrUserName" 
              name="emailOrUserName"
              class="mt-1 block w-full px-3 py-2 border border-gray-600 rounded-md bg-gray-800 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-mafia-gold"
              required
            >
          </div>
          
          <div>
            <label class="block text-sm font-medium text-gray-300">Şifre</label>
            <input 
              type="password" 
              [(ngModel)]="credentials.password" 
              name="password"
              class="mt-1 block w-full px-3 py-2 border border-gray-600 rounded-md bg-gray-800 text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-mafia-gold"
              required
            >
          </div>
          
          <div>
            <button 
              type="submit" 
              [disabled]="loading"
              class="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-mafia-accent hover:bg-red-800 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-mafia-gold disabled:opacity-50"
            >
              {{ loading ? 'Giriş yapılıyor...' : 'Giriş Yap' }}
            </button>
          </div>
          
          <div *ngIf="error" class="text-red-400 text-sm text-center">
            {{ error }}
          </div>
        </form>
        
        <div class="text-center">
          <a routerLink="/auth/register" class="text-mafia-gold hover:text-yellow-400">
            Hesabın yok mu? Kayıt ol
          </a>
        </div>
      </div>
    </div>
  `
})
export class LoginComponent {
  credentials: LoginRequest = {
    emailOrUserName: '',
    password: ''
  };
  
  loading = false;
  error = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  async onLogin(): Promise<void> {
    if (!this.credentials.emailOrUserName || !this.credentials.password) {
      this.error = 'Lütfen tüm alanları doldurun';
      return;
    }

    this.loading = true;
    this.error = '';

    try {
      await this.authService.login(this.credentials);
      this.router.navigate(['/lobby']);
    } catch (error) {
      this.error = 'Giriş başarısız. Lütfen bilgilerinizi kontrol edin.';
      console.error('Login error:', error);
    } finally {
      this.loading = false;
    }
  }
}
