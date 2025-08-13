import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, from } from 'rxjs';
import { AuthResponse, LoginRequest, RegisterRequest, RefreshRequest } from '../../shared/models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private tokenSubject = new BehaviorSubject<string | null>(this.getStoredToken());
  private refreshInProgress = false;
  private refreshQueue: Array<() => void> = [];

  public token$ = this.tokenSubject.asObservable();

  constructor() {}

  get token(): string | null {
    return this.tokenSubject.value;
  }

  private getStoredToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  private setStoredToken(token: string | null): void {
    if (token) {
      localStorage.setItem('accessToken', token);
    } else {
      localStorage.removeItem('accessToken');
    }
  }

  private getStoredRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

  private setStoredRefreshToken(token: string | null): void {
    if (token) {
      localStorage.setItem('refreshToken', token);
    } else {
      localStorage.removeItem('refreshToken');
    }
  }

  isExpiredSoon(offsetSec: number = 30): boolean {
    const token = this.token;
    if (!token) return true;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const exp = payload.exp * 1000; // Convert to milliseconds
      const now = Date.now();
      return (exp - now) < (offsetSec * 1000);
    } catch {
      return true;
    }
  }

  isAuthenticated(): boolean {
    const token = this.token;
    if (!token) return false;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.exp * 1000 > Date.now();
    } catch {
      return false;
    }
  }

  hasRole(role: string): boolean {
    const token = this.token;
    if (!token) return false;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.role?.includes(role) || false;
    } catch {
      return false;
    }
  }

  login(credentials: LoginRequest): Promise<AuthResponse> {
    return fetch(`${environment.apiBaseUrl}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(credentials)
    })
    .then(response => {
      if (!response.ok) throw new Error('Login failed');
      return response.json();
    })
    .then((authResponse: AuthResponse) => {
      this.setTokens(authResponse);
      return authResponse;
    });
  }

  register(request: RegisterRequest): Promise<AuthResponse> {
    return fetch(`${environment.apiBaseUrl}/auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request)
    })
    .then(response => {
      if (!response.ok) throw new Error('Registration failed');
      return response.json();
    })
    .then((authResponse: AuthResponse) => {
      this.setTokens(authResponse);
      return authResponse;
    });
  }

  refresh(): Observable<string> {
    const refreshToken = this.getStoredRefreshToken();
    if (!refreshToken) {
      return from(Promise.reject(new Error('No refresh token')));
    }

    if (this.refreshInProgress) {
      return from(new Promise<string>((resolve, reject) => {
        this.refreshQueue.push(() => {
          const token = this.token;
          if (token) resolve(token);
          else reject(new Error('Refresh failed'));
        });
      }));
    }

    this.refreshInProgress = true;

    return from(fetch(`${environment.apiBaseUrl}/auth/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken } as RefreshRequest)
    })
    .then(response => {
      if (!response.ok) throw new Error('Refresh failed');
      return response.json();
    })
    .then((authResponse: AuthResponse) => {
      this.setTokens(authResponse);
      this.refreshInProgress = false;
      
      // Process queued requests
      this.refreshQueue.forEach(callback => callback());
      this.refreshQueue = [];
      
      return authResponse.accessToken;
    })
    .catch(error => {
      this.refreshInProgress = false;
      this.refreshQueue.forEach(callback => callback());
      this.refreshQueue = [];
      throw error;
    }));
  }

  private setTokens(authResponse: AuthResponse): void {
    this.setStoredToken(authResponse.accessToken);
    this.setStoredRefreshToken(authResponse.refreshToken);
    this.tokenSubject.next(authResponse.accessToken);
  }

  logout(): void {
    this.setStoredToken(null);
    this.setStoredRefreshToken(null);
    this.tokenSubject.next(null);
  }
}

// Import environment
import { environment } from '../../../environments/environment';
