import { Injectable, signal, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../../environments/environment';
import { AuthService } from '../../../core/services/auth.service';
import { SessionService } from './session.service';
import { AiStreamChunk, AiConversationSummary } from '../models/ai.models';
import { Subject, Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AiSignalrService {
  private authService = inject(AuthService);
  private sessionService = inject(SessionService);

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

  private pendingMessages: Array<{ method: string; args: unknown[] }> = [];
  private connectionPromise: Promise<void> | null = null;

  async startConnection(): Promise<void> {
    const token = this.authService.getAccessToken();
    this.connectionPromise = this.establishConnection(token);
    await this.connectionPromise;
  }

  private async establishConnection(token: string | null): Promise<void> {
    const baseUrl = environment.apiUrl.replace('/api/v1', '');
    const sessionId = this.sessionService.getSessionId();
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${baseUrl}/hubs/ai?sessionId=${encodeURIComponent(sessionId)}`, {
        accessTokenFactory: () => token ?? '',
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents | signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .build();

    this.registerHandlers();

    try {
      await this.hubConnection.start();
      this.connectionState.set(signalR.HubConnectionState.Connected);
      this.flushPendingMessages();
    } catch {
      this.connectionState.set(signalR.HubConnectionState.Disconnected);
    }
  }

  async stopConnection(): Promise<void> {
    this.pendingMessages = [];
    this.connectionPromise = null;
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.hubConnection = null;
      this.connectionState.set(signalR.HubConnectionState.Disconnected);
    }
  }

  async sendMessage(conversationId: number, content: string): Promise<void> {
    const sessionId = this.sessionService.getSessionId();
    await this.ensureConnected();
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('SendMessage', conversationId, content, sessionId);
    }
  }

  async startConversation(title?: string, firstMessage?: string): Promise<void> {
    const sessionId = this.sessionService.getSessionId();
    await this.ensureConnected();
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('StartConversation', title ?? null, firstMessage ?? null, sessionId);
    }
  }

  async deleteConversation(conversationId: number): Promise<void> {
    await this.ensureConnected();
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('DeleteConversation', conversationId);
    }
  }

  private async ensureConnected(): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) return;

    if (this.hubConnection?.state === signalR.HubConnectionState.Reconnecting) {
      try {
        await this.hubConnection.stop();
      } catch { /* ignore */ }
    }

    if (this.connectionPromise) {
      await this.connectionPromise;
    } else {
      await this.startConnection();
    }
  }

  private flushPendingMessages(): void {
    const msgs = [...this.pendingMessages];
    this.pendingMessages = [];
    for (const msg of msgs) {
      this.hubConnection?.invoke(msg.method, ...msg.args).catch(() => {});
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
