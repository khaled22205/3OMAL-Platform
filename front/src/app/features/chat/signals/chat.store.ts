import { Injectable, signal, computed, inject } from '@angular/core';
import { ChatApiService } from '../services/chat.service';
import { SignalrService } from '../services/signalr.service';
import { AuthService } from '../../../core/services/auth.service';
import {
  ConversationResponse,
  MessageResponse,
  SendMessageRequest,
} from '../models/chat.models';
import { Subscription } from 'rxjs';

interface MessagesPage {
  page: number;
  hasMore: boolean;
  items: MessageResponse[];
}

@Injectable({ providedIn: 'root' })
export class ChatStore {
  private api = inject(ChatApiService);
  private signalr = inject(SignalrService);
  private authService = inject(AuthService);

  readonly conversations = signal<ConversationResponse[]>([]);
  readonly conversationsPage = signal(1);
  readonly hasMoreConversations = signal(true);
  readonly conversationsLoading = signal(false);

  readonly activeConversation = signal<ConversationResponse | null>(null);
  readonly messages = signal<MessageResponse[]>([]);
  readonly messagesPage = signal(1);
  readonly hasMoreMessages = signal(true);
  readonly messagesLoading = signal(false);
  readonly sendingMessage = signal(false);

  readonly typingUserIds = signal<Set<number>>(new Set());
  readonly onlineUserIds = signal<Set<number>>(new Set());
  readonly unreadCount = signal(0);
  readonly replyToMessage = signal<MessageResponse | null>(null);

  readonly currentUserId = computed(() => {
    const u = this.authService.user();
    return u ? u.id : 0;
  });

  private subscriptions: Subscription[] = [];

  init(): void {
    this.signalr.startConnection().then(() => {
      this.loadConversations();
      this.loadUnreadCount();
    });

    this.subscriptions.push(
      this.signalr.onNewMessage$.subscribe(msg => this.handleNewMessage(msg)),
      this.signalr.onMessageEdited$.subscribe(msg => this.handleMessageEdited(msg)),
      this.signalr.onMessageDeleted$.subscribe(data => this.handleMessageDeleted(data.messageId)),
      this.signalr.onMessagesRead$.subscribe(data => this.handleMessagesRead(data.conversationId, data.messageIds)),
      this.signalr.onUserTyping$.subscribe(data => this.handleUserTyping(data.conversationId, data.userId)),
      this.signalr.onUserStoppedTyping$.subscribe(data => this.handleUserStoppedTyping(data.conversationId, data.userId)),
      this.signalr.onUserOnline$.subscribe(userId => this.handleUserOnline(userId)),
      this.signalr.onUserOffline$.subscribe(userId => this.handleUserOffline(userId)),
    );
  }

  destroy(): void {
    this.subscriptions.forEach(s => s.unsubscribe());
    this.signalr.stopConnection();
  }

  selectConversation(conv: ConversationResponse): void {
    const prevConv = this.activeConversation();
    if (prevConv && prevConv.id !== conv.id) {
      this.signalr.leaveConversationGroup(prevConv.id);
    }
    this.activeConversation.set(conv);
    this.messages.set([]);
    this.messagesPage.set(1);
    this.hasMoreMessages.set(true);
    this.replyToMessage.set(null);
    this.signalr.joinConversationGroup(conv.id);
    this.loadMessages();
    this.markAsRead(conv);
  }

  async loadConversations(): Promise<void> {
    if (this.conversationsLoading() || !this.hasMoreConversations()) return;
    this.conversationsLoading.set(true);
    try {
      const result = await this.api.getConversations(this.conversationsPage(), 20).toPromise();
      if (result) {
        if (this.conversationsPage() === 1) {
          this.conversations.set(result.items);
        } else {
          this.conversations.update(items => [...items, ...result.items]);
        }
        this.hasMoreConversations.set(result.hasNextPage);
        this.conversationsPage.update(p => p + 1);
      }
    } finally {
      this.conversationsLoading.set(false);
    }
  }

  async loadMessages(): Promise<void> {
    const conv = this.activeConversation();
    if (!conv || this.messagesLoading() || !this.hasMoreMessages()) return;
    this.messagesLoading.set(true);
    try {
      const result = await this.api.getMessages(conv.id, this.messagesPage(), 50).toPromise();
      if (result) {
        if (this.messagesPage() === 1) {
          this.messages.set(result.items);
        } else {
          this.messages.update(items => [...result.items, ...items]);
        }
        this.hasMoreMessages.set(result.hasNextPage);
        this.messagesPage.update(p => p + 1);
      }
    } finally {
      this.messagesLoading.set(false);
    }
  }

  async sendMessage(content: string, messageType = 'Text'): Promise<void> {
    const conv = this.activeConversation();
    if (!conv || this.sendingMessage()) return;
    this.sendingMessage.set(true);
    const replyTo = this.replyToMessage();
    try {
      const request: SendMessageRequest = {
        conversationId: conv.id,
        messageType,
        content,
        replyToMessageId: replyTo?.id ?? null,
      };
      await this.signalr.sendMessage(request);
      this.replyToMessage.set(null);
    } finally {
      this.sendingMessage.set(false);
    }
  }

  async editMessage(messageId: number, content: string): Promise<void> {
    await this.signalr.editMessage(messageId, { content });
  }

  async deleteMessage(messageId: number): Promise<void> {
    await this.signalr.deleteMessage(messageId);
  }

  markAsRead(conv: ConversationResponse): void {
    const unreadMessages = this.messages()
      .filter(m => m.senderId !== this.currentUserId() && !m.readAt)
      .map(m => m.id);
    if (unreadMessages.length > 0) {
      this.signalr.markAsRead(conv.id, unreadMessages);
    }
  }

  setReplyTo(msg: MessageResponse | null): void {
    this.replyToMessage.set(msg);
  }

  startTyping(): void {
    const conv = this.activeConversation();
    if (conv) this.signalr.startTyping(conv.id);
  }

  stopTyping(): void {
    const conv = this.activeConversation();
    if (conv) this.signalr.stopTyping(conv.id);
  }

  async loadUnreadCount(): Promise<void> {
    try {
      const result = await this.api.getUnreadCount().toPromise();
      if (result) this.unreadCount.set(result.count);
    } catch { }
  }

  async searchConversations(query: string): Promise<ConversationResponse[]> {
    try {
      const result = await this.api.searchConversations(query, 1, 50).toPromise();
      return result?.items ?? [];
    } catch {
      return [];
    }
  }

  async searchMessages(query: string): Promise<MessageResponse[]> {
    try {
      const result = await this.api.searchMessages(query, 1, 50).toPromise();
      return result?.items ?? [];
    } catch {
      return [];
    }
  }

  private handleNewMessage(msg: MessageResponse): void {
    const activeConv = this.activeConversation();
    if (activeConv && msg.conversationId === activeConv.id) {
      this.messages.update(items => [...items, msg]);
      this.markAsRead(activeConv);
    }
    this.conversations.update(convs => {
      const idx = convs.findIndex(c => c.id === msg.conversationId);
      if (idx >= 0) {
        const updated = { ...convs[idx], lastMessage: msg, lastMessageAt: msg.createdAt };
        const newConvs = [...convs];
        newConvs[idx] = updated;
        return newConvs.sort((a, b) => {
          if (!a.lastMessageAt) return 1;
          if (!b.lastMessageAt) return -1;
          return new Date(b.lastMessageAt).getTime() - new Date(a.lastMessageAt).getTime();
        });
      }
      return convs;
    });
    this.loadUnreadCount();
  }

  private handleMessageEdited(msg: MessageResponse): void {
    this.messages.update(items =>
      items.map(m => m.id === msg.id ? { ...m, content: msg.content, isEdited: true, editedAt: msg.editedAt } : m)
    );
    this.conversations.update(convs =>
      convs.map(c => c.lastMessage?.id === msg.id ? { ...c, lastMessage: { ...c.lastMessage!, content: msg.content } } : c)
    );
  }

  private handleMessageDeleted(messageId: number): void {
    this.messages.update(items =>
      items.map(m => m.id === messageId ? { ...m, isDeleted: true, content: 'تم حذف الرسالة' } : m)
    );
  }

  private handleMessagesRead(conversationId: number, messageIds: number[]): void {
    const idSet = new Set(messageIds);
    this.messages.update(items =>
      items.map(m => idSet.has(m.id) ? { ...m, readAt: new Date().toISOString() } : m)
    );
    this.conversations.update(convs =>
      convs.map(c => c.id === conversationId ? { ...c, unreadCount: 0 } : c)
    );
  }

  private handleUserTyping(conversationId: number, userId: number): void {
    const activeConv = this.activeConversation();
    if (activeConv?.id === conversationId && userId !== this.currentUserId()) {
      this.typingUserIds.update(set => new Set(set).add(userId));
    }
  }

  private handleUserStoppedTyping(conversationId: number, userId: number): void {
    const activeConv = this.activeConversation();
    if (activeConv?.id === conversationId) {
      this.typingUserIds.update(set => {
        const newSet = new Set(set);
        newSet.delete(userId);
        return newSet;
      });
    }
  }

  private handleUserOnline(userId: number): void {
    this.onlineUserIds.update(set => new Set(set).add(userId));
  }

  private handleUserOffline(userId: number): void {
    this.onlineUserIds.update(set => {
      const newSet = new Set(set);
      newSet.delete(userId);
      return newSet;
    });
  }
}
