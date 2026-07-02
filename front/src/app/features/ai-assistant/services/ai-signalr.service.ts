import { Injectable, signal, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../../environments/environment';
import { AuthService } from '../../../core/services/auth.service';
import { AiStreamChunk, AiConversationSummary } from '../models/ai.models';
import { Subject, Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AiSignalrService {
  private authService = inject(AuthService);

  private hubConnection: signalR.HubConnection | null = null;
  readonly connectionState = signal<signalR.HubConnectionState>(
    signalR.HubConnectionState.Disconnected,
  );

  private chunkSubject = new Subject<AiStreamChunk>();
  private errorSubject = new Subject<string>();
  private conversationCreatedSubject = new Subject<AiConversationSummary>();

  readonly onChunk$: Observable<AiStreamChunk> = this.chunkSubject.asObservable();
  readonly onError$: Observable<string> = this.errorSubject.asObservable();
  readonly onConversationCreated$: Observable<AiConversationSummary> =
    this.conversationCreatedSubject.asObservable();

  async startConnection(): Promise<void> {
    const token = this.authService.getAccessToken();
    if (!token) return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl.replace('/api/v1', '')}/hubs/ai`, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .build();

    this.registerHandlers();

    try {
      await this.hubConnection.start();
      this.connectionState.set(signalR.HubConnectionState.Connected);
    } catch {
      this.connectionState.set(signalR.HubConnectionState.Disconnected);
    }
  }

  async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.hubConnection = null;
      this.connectionState.set(signalR.HubConnectionState.Disconnected);
    }
  }

  async sendMessage(conversationId: number, content: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('SendMessage', conversationId, content);
    }
  }

  async startConversation(title?: string, firstMessage?: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('StartConversation', title ?? null, firstMessage ?? null);
    }
  }

  async deleteConversation(conversationId: number): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('DeleteConversation', conversationId);
    }
  }

  private registerHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('AiResponseChunk', (chunk: AiStreamChunk) =>
      this.chunkSubject.next(chunk),
    );
    this.hubConnection.on('AiResponseError', (error: string) => this.errorSubject.next(error));
    this.hubConnection.on('AiConversationCreated', (conv: AiConversationSummary) =>
      this.conversationCreatedSubject.next(conv),
    );

    this.hubConnection.onreconnecting(() =>
      this.connectionState.set(signalR.HubConnectionState.Reconnecting),
    );
    this.hubConnection.onreconnected(async () => {
      this.connectionState.set(signalR.HubConnectionState.Connected);
    });
    this.hubConnection.onclose(() =>
      this.connectionState.set(signalR.HubConnectionState.Disconnected),
    );
  }
}
