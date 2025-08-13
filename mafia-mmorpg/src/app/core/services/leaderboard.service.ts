import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LeaderboardEntryDto } from '../../shared/models/leaderboard.models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class LeaderboardService {
  private apiUrl = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  getGlobalLeaderboard(top: number = 1000): Observable<LeaderboardEntryDto[]> {
    return this.http.get<LeaderboardEntryDto[]>(`${this.apiUrl}/leaderboard/global?top=${top}`);
  }

  getRegionalLeaderboard(region: string, top: number = 1000): Observable<LeaderboardEntryDto[]> {
    return this.http.get<LeaderboardEntryDto[]>(`${this.apiUrl}/leaderboard/region/${region}?top=${top}`);
  }
}
