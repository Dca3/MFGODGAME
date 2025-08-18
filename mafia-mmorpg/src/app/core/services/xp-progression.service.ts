import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface XpProgressionData {
  totalLevels: number;
  totalXpNeeded: number;
  levelProgression: {
    level: number;
    xpToNext: number;
    totalXp: number;
  }[];
}

@Injectable({
  providedIn: 'root'
})
export class XpProgressionService {
  private apiUrl = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  getXpProgression(): Observable<XpProgressionData> {
    return this.http.get<XpProgressionData>(`${this.apiUrl}/xp-progression`);
  }
}
