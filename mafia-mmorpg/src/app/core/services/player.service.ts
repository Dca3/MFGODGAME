import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PlayerProfileDto, PlayerStatsDto, AllocateStatsRequest } from '../../shared/models/player.models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PlayerService {
  private apiUrl = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  getProfile(): Observable<PlayerProfileDto> {
    return this.http.get<PlayerProfileDto>(`${this.apiUrl}/me`);
  }

  getStats(): Observable<PlayerStatsDto> {
    return this.http.get<PlayerStatsDto>(`${this.apiUrl}/me/stats`);
  }

  allocateStats(request: AllocateStatsRequest): Observable<PlayerStatsDto> {
    return this.http.post<PlayerStatsDto>(`${this.apiUrl}/me/stats/allocate`, request);
  }
}
