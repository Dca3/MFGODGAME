import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { QuestDto, QuestCompleteResult } from '../../shared/models/quest.models';

@Injectable({
  providedIn: 'root'
})
export class QuestsService {
  private apiUrl = `${environment.apiBaseUrl}/quests`;

  constructor(private http: HttpClient) {}

  getAvailable(): Observable<QuestDto[]> {
    return this.http.get<QuestDto[]>(`${this.apiUrl}/available`);
  }

  start(questId: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/${questId}/start`, {});
  }

  complete(questId: string): Observable<QuestCompleteResult> {
    return this.http.post<QuestCompleteResult>(`${this.apiUrl}/${questId}/complete`, {});
  }
}
