import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PvpService {
  private apiUrl = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  joinQueue(): Observable<any> {
    return this.http.post(`${this.apiUrl}/pvp/queue`, {});
  }

  leaveQueue(): Observable<any> {
    return this.http.delete(`${this.apiUrl}/pvp/queue`);
  }

  getQueueStatus(): Observable<any> {
    return this.http.get(`${this.apiUrl}/pvp/status`);
  }
}
