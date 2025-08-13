import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { MatchInfo, QueueState } from '../../shared/models/pvp.models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DuelHubService {
  private hubConnection: HubConnection | null = null;
  private isConnected = false;

  // State subjects
  private queueStateSubject = new BehaviorSubject<QueueState>('idle');
  private matchSubject = new BehaviorSubject<MatchInfo | null>(null);
  private duelStateSubject = new BehaviorSubject<any>(null);
  private resultSubject = new BehaviorSubject<any>(null);
  private errorSubject = new BehaviorSubject<string | null>(null);

  // Public observables
  public queueState$ = this.queueStateSubject.asObservable();
  public match$ = this.matchSubject.asObservable();
  public duelState$ = this.duelStateSubject.asObservable();
  public result$ = this.resultSubject.asObservable();
  public error$ = this.errorSubject.asObservable();

  constructor(private authService: AuthService) {}

  async start(): Promise<void> {
    if (this.isConnected) return;

    const token = this.authService.token;
    if (!token) {
      throw new Error('No authentication token');
    }

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.hubUrl}?access_token=${token}`)
      .withAutomaticReconnect()
      .build();

    this.setupEventHandlers();

    try {
      await this.hubConnection.start();
      this.isConnected = true;
      console.log('SignalR Hub connected');
    } catch (error) {
      console.error('SignalR Hub connection failed:', error);
      throw error;
    }
  }

  async stop(): Promise<void> {
    if (this.hubConnection && this.isConnected) {
      await this.hubConnection.stop();
      this.isConnected = false;
      this.resetState();
      console.log('SignalR Hub disconnected');
    }
  }

  private setupEventHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('MatchFound', (matchInfo: MatchInfo) => {
      console.log('Match found:', matchInfo);
      this.matchSubject.next(matchInfo);
      this.queueStateSubject.next('matched');
    });

    this.hubConnection.on('QueueJoined', (message: string) => {
      console.log('Queue joined:', message);
      this.queueStateSubject.next('queued');
    });

    this.hubConnection.on('QueueLeft', (message: string) => {
      console.log('Queue left:', message);
      this.queueStateSubject.next('idle');
    });

    this.hubConnection.on('MatchAccepted', (matchId: string) => {
      console.log('Match accepted:', matchId);
      this.queueStateSubject.next('inDuel');
    });

    this.hubConnection.on('DuelStateUpdated', (state: any) => {
      console.log('Duel state updated:', state);
      this.duelStateSubject.next(state);
    });

    this.hubConnection.on('DuelEnded', (result: any) => {
      console.log('Duel ended:', result);
      this.resultSubject.next(result);
      this.queueStateSubject.next('idle');
    });

    this.hubConnection.on('Error', (error: string) => {
      console.error('Hub error:', error);
      this.errorSubject.next(error);
    });

    this.hubConnection.onclose(() => {
      console.log('Hub connection closed');
      this.isConnected = false;
      this.resetState();
    });
  }

  private resetState(): void {
    this.queueStateSubject.next('idle');
    this.matchSubject.next(null);
    this.duelStateSubject.next(null);
    this.resultSubject.next(null);
    this.errorSubject.next(null);
  }

  async joinQueue(): Promise<void> {
    if (!this.hubConnection || !this.isConnected) {
      throw new Error('Hub not connected');
    }
    await this.hubConnection.invoke('JoinQueue');
  }

  async leaveQueue(): Promise<void> {
    if (!this.hubConnection || !this.isConnected) {
      throw new Error('Hub not connected');
    }
    await this.hubConnection.invoke('LeaveQueue');
  }

  async acceptMatch(matchId: string): Promise<void> {
    if (!this.hubConnection || !this.isConnected) {
      throw new Error('Hub not connected');
    }
    await this.hubConnection.invoke('AcceptMatch', matchId);
  }

  async submitAction(action: any): Promise<void> {
    if (!this.hubConnection || !this.isConnected) {
      throw new Error('Hub not connected');
    }
    await this.hubConnection.invoke('SubmitAction', action);
  }

  getConnectionState(): boolean {
    return this.isConnected;
  }
}
