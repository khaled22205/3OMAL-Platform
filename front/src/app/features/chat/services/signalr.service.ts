import { Injectable, signal, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../../environments/environment';
import { AuthService } from '../../../core/services/auth.service';
import { MessageResponse, ConversationResponse } from '../models/chat.models';
import { Subject, Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class SignalrService {
  private authService = inject(AuthService);

  private hubConnection: signalR.HubConnection | null = null;
  readonly connectionState = signal<signalR.HubConnectionState>(signalR.HubConnectionState.Disconnected);

  private newMessageSubject = new Subject<MessageResponse>();
  private messageEditedSubject = new Subject<MessageResponse>();
  private messageDeletedSubject = new Subject<{ messageId: number; userId: number }>();
  private messagesReadSubject = new Subject<{ conversationId: number; readByUserId: number; messageIds: number[] }>();
  private userTypingSubject = new Subject<{ conversationId: number; userId: number }>();
  private userStoppedTypingSubject = new Subject<{ conversationId: number; userId: number }>();
  private userOnlineSubject = new Subject<number>();
  private userOfflineSubject = new Subject<number>();

  readonly onNewMessage$: Observable<MessageResponse> = this.newMessageSubject.asObservable();
  readonly onMessageEdited$: Observable<MessageResponse> = this.messageEditedSubject.asObservable();
  readonly onMessageDeleted$: Observable<{ messageId: number; userId: number }> = this.messageDeletedSubject.asObservable();
  readonly onMessagesRead$: Observable<{ conversationId: number; readByUserId: number; messageIds: number[] }> = this.messagesReadSubject.asObservable();
  readonly onUserTyping$: Observable<{ conversationId: number; userId: number }> = this.userTypingSubject.asObservable();
  readonly onUserStoppedTyping$: Observable<{ conversationId: number; userId: number }> = this.userStoppedTypingSubject.asObservable();
  readonly onUserOnline$: Observable<number> = this.userOnlineSubject.asObservable();
  readonly onUserOffline$: Observable<number> = this.userOfflineSubject.asObservable();

  async startConnection(): Promise<void> {
    const token = this.authService.getAccessToken();
    if (!token) return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl.replace('/api/v1', '')}/hubs/chat`, {
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

  async joinConversationGroup(conversationId: number): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('JoinConversationGroup', conversationId);
    }
  }

  async leaveConversationGroup(conversationId: number): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('LeaveConversationGroup', conversationId);
    }
  }

  async sendMessage(request: { conversationId: number; messageType: string; content: string | null; replyToMessageId: number | null }): Promise<void> {
    await this.hubConnection?.invoke('SendMessage', request);
  }

  async editMessage(messageId: number, request: { content: string }): Promise<void> {
    await this.hubConnection?.invoke('EditMessage', messageId, request);
  }

  async deleteMessage(messageId: number): Promise<void> {
    await this.hubConnection?.invoke('DeleteMessage', messageId);
  }

  async markAsRead(conversationId: number, messageIds: number[]): Promise<void> {
    await this.hubConnection?.invoke('MarkAsRead', { conversationId, messageIds });
  }

  async startTyping(conversationId: number): Promise<void> {
    await this.hubConnection?.invoke('StartTyping', conversationId);
  }

  async stopTyping(conversationId: number): Promise<void> {
    await this.hubConnection?.invoke('StopTyping', conversationId);
  }

  private registerHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('NewMessage', (msg: MessageResponse) => this.newMessageSubject.next(msg));
    this.hubConnection.on('MessageEdited', (msg: MessageResponse) => this.messageEditedSubject.next(msg));
    this.hubConnection.on('MessageDeleted', (messageId: number, userId: number) =>
      this.messageDeletedSubject.next({ messageId, userId }));
    this.hubConnection.on('MessagesRead', (conversationId: number, readByUserId: number, messageIds: number[]) =>
      this.messagesReadSubject.next({ conversationId, readByUserId, messageIds }));
    this.hubConnection.on('UserTyping', (conversationId: number, userId: number) =>
      this.userTypingSubject.next({ conversationId, userId }));
    this.hubConnection.on('UserStoppedTyping', (conversationId: number, userId: number) =>
      this.userStoppedTypingSubject.next({ conversationId, userId }));
    this.hubConnection.on('UserOnline', (userId: number) => this.userOnlineSubject.next(userId));
    this.hubConnection.on('UserOffline', (userId: number) => this.userOfflineSubject.next(userId));

    this.hubConnection.onreconnecting(() => this.connectionState.set(signalR.HubConnectionState.Reconnecting));
    this.hubConnection.onreconnected(async () => {
      this.connectionState.set(signalR.HubConnectionState.Connected);
    });
    this.hubConnection.onclose(() => this.connectionState.set(signalR.HubConnectionState.Disconnected));
  }
}
