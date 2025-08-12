import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: HubConnection;
  private connectionState$ = new BehaviorSubject<boolean>(false);
  private duelUpdates$ = new BehaviorSubject<any>(null);
  private matchFound$ = new BehaviorSubject<any>(null);

  constructor() {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/duelHub`)
      .configureLogging(LogLevel.Information)
      .build();

    this.setupConnectionHandlers();
  }

  private setupConnectionHandlers(): void {
    this.hubConnection.onclose((error) => {
      console.log('SignalR connection closed:', error);
      this.connectionState$.next(false);
    });

    this.hubConnection.onreconnecting((error) => {
      console.log('SignalR reconnecting:', error);
    });

    this.hubConnection.onreconnected((connectionId) => {
      console.log('SignalR reconnected:', connectionId);
      this.connectionState$.next(true);
    });

    // Duel specific handlers
    this.hubConnection.on('MatchFound', (matchData) => {
      console.log('Match found:', matchData);
      this.matchFound$.next(matchData);
    });

    this.hubConnection.on('DuelStateUpdated', (duelState) => {
      console.log('Duel state updated:', duelState);
      this.duelUpdates$.next(duelState);
    });

    this.hubConnection.on('DuelEnded', (result) => {
      console.log('Duel ended:', result);
      this.duelUpdates$.next({ type: 'ended', data: result });
    });
  }

  async startConnection(): Promise<void> {
    try {
      await this.hubConnection.start();
      console.log('SignalR connection established');
      this.connectionState$.next(true);
    } catch (error) {
      console.error('SignalR connection failed:', error);
      this.connectionState$.next(false);
    }
  }

  async stopConnection(): Promise<void> {
    try {
      await this.hubConnection.stop();
      console.log('SignalR connection stopped');
      this.connectionState$.next(false);
    } catch (error) {
      console.error('SignalR stop failed:', error);
    }
  }

  // Duel methods
  async joinQueue(): Promise<void> {
    try {
      await this.hubConnection.invoke('JoinQueue');
    } catch (error) {
      console.error('Join queue failed:', error);
      throw error;
    }
  }

  async leaveQueue(): Promise<void> {
    try {
      await this.hubConnection.invoke('LeaveQueue');
    } catch (error) {
      console.error('Leave queue failed:', error);
      throw error;
    }
  }

  async submitDuelAction(action: any): Promise<void> {
    try {
      await this.hubConnection.invoke('SubmitAction', action);
    } catch (error) {
      console.error('Submit action failed:', error);
      throw error;
    }
  }

  // Observables
  get connectionState(): Observable<boolean> {
    return this.connectionState$.asObservable();
  }

  get duelUpdates(): Observable<any> {
    return this.duelUpdates$.asObservable();
  }

  get matchFound(): Observable<any> {
    return this.matchFound$.asObservable();
  }

  get isConnected(): boolean {
    return this.connectionState$.value;
  }
}
