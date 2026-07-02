import { Injectable, signal, computed, inject } from '@angular/core';
import { AiApiService } from './ai-api.service';
import { AiSignalrService } from './ai-signalr.service';
import { AuthService } from '../../../core/services/auth.service';
import {
  AiConversationSummary,
  AiMessage,
  AiStreamChunk,
} from '../models/ai.models';
import { Subscription } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AiStore {
  private api = inject(AiApiService);
  private signalr = inject(AiSignalrService);
  private authService = inject(AuthService);

  readonly isOpen = signal(false);
  readonly conversations = signal<AiConversationSummary[]>([]);
  readonly conversationsPage = signal(1);
  readonly hasMoreConversations = signal(true);
  readonly conversationsLoading = signal(false);

  readonly activeConversationId = signal<number | null>(null);
  readonly activeConversation = signal<AiConversationSummary | null>(null);
  readonly messages = signal<AiMessage[]>([]);
  readonly messagesLoading = signal(false);

  readonly isStreaming = signal(false);
  readonly streamingContent = signal('');
  readonly error = signal<string | null>(null);
  readonly suggestedPrompts = signal<string[]>([]);
  readonly searchQuery = signal('');

  readonly currentUserRole = computed(() => {
    const u = this.authService.user();
    if (this.authService.isAdmin()) return 'Admin';
    if (this.authService.isWorker()) return 'Worker';
    if (this.authService.isCustomer()) return 'Customer';
    return 'Guest';
  });

  readonly isAuthenticated = computed(() => this.authService.isAuthenticated());

  readonly filteredConversations = computed(() => {
    const q = this.searchQuery().toLowerCase();
    if (!q) return this.conversations();
    return this.conversations().filter((c) => c.title.toLowerCase().includes(q));
  });

  private subscriptions: Subscription[] = [];
  private pendingContent = '';

  toggle(): void {
    if (!this.isOpen()) {
      this.isOpen.set(true);
      this.loadConversations();
      this.loadSuggestions();
    } else {
      this.isOpen.set(false);
    }
  }

  open(): void {
    if (!this.isOpen()) {
      this.isOpen.set(true);
      this.loadConversations();
      this.loadSuggestions();
    }
  }

  close(): void {
    this.isOpen.set(false);
  }

  init(): void {
    this.signalr.startConnection();

    this.subscriptions.push(
      this.signalr.onChunk$.subscribe((chunk) => this.handleChunk(chunk)),
      this.signalr.onError$.subscribe((error) => {
        this.error.set(error);
        this.isStreaming.set(false);
      }),
      this.signalr.onConversationCreated$.subscribe((conv) => {
        this.handleNewConversation(conv);
      }),
    );
  }

  destroy(): void {
    this.subscriptions.forEach((s) => s.unsubscribe());
    this.signalr.stopConnection();
  }

  async loadConversations(page = 1): Promise<void> {
    if (this.conversationsLoading() || !this.hasMoreConversations()) return;
    this.conversationsLoading.set(true);
    try {
      const result = await this.api.getConversations(page, 20).toPromise();
      if (result) {
        if (page === 1) {
          this.conversations.set(result.items);
        } else {
          this.conversations.update((items) => [...items, ...result.items]);
        }
        this.hasMoreConversations.set(result.hasNextPage);
        this.conversationsPage.set(page + 1);
      }
    } finally {
      this.conversationsLoading.set(false);
    }
  }

  async loadSuggestions(): Promise<void> {
    try {
      const result = await this.api.getSuggestions().toPromise();
      if (result) this.suggestedPrompts.set(result.prompts);
    } catch {}
  }

  async selectConversation(id: number): Promise<void> {
    this.activeConversationId.set(id);
    this.messages.set([]);
    this.error.set(null);

    const conv = this.conversations().find((c) => c.id === id);
    this.activeConversation.set(conv ?? null);

    try {
      const detail = await this.api.getConversation(id).toPromise();
      if (detail) {
        this.messages.set(detail.messages);
      }
    } catch {
      this.error.set('Failed to load conversation');
    }
  }

  async sendMessage(content: string): Promise<void> {
    const convId = this.activeConversationId();
    if (!convId || !content.trim() || this.isStreaming()) return;

    const userMsg: AiMessage = {
      id: 0,
      conversationId: convId,
      role: 'User',
      content: content.trim(),
      sources: [],
      createdAt: new Date().toISOString(),
    };
    this.messages.update((msgs) => [...msgs, userMsg]);
    this.isStreaming.set(true);
    this.streamingContent.set('');
    this.pendingContent = '';
    this.error.set(null);

    await this.signalr.sendMessage(convId, content.trim());
  }

  async startNewConversation(firstMessage?: string): Promise<void> {
    this.error.set(null);

    if (this.isAuthenticated()) {
      try {
        const conv = await this.api.startConversation({
          title: firstMessage ? firstMessage.slice(0, 100) : undefined,
          firstMessage,
        }).toPromise();

        if (conv) {
          this.handleNewConversation(conv);
          if (firstMessage) {
            setTimeout(() => this.sendMessage(firstMessage), 100);
          }
          return;
        }
      } catch {
        this.error.set('Failed to start conversation');
      }
    }

    this.messages.set([]);
    this.activeConversationId.set(null);
    this.activeConversation.set(null);
  }

  async deleteConversation(id: number): Promise<void> {
    try {
      await this.api.deleteConversation(id).toPromise();
      this.conversations.update((convs) => convs.filter((c) => c.id !== id));
      if (this.activeConversationId() === id) {
        this.activeConversationId.set(null);
        this.activeConversation.set(null);
        this.messages.set([]);
      }
    } catch {}
  }

  retryLastMessage(): void {
    const msgs = this.messages();
    const lastUserMsg = [...msgs].reverse().find((m) => m.role === 'User');
    if (lastUserMsg && this.activeConversationId()) {
      this.messages.update((msgs) => msgs.filter((m) => m.id !== 0 || m.role !== 'Assistant'));
      this.isStreaming.set(true);
      this.streamingContent.set('');
      this.pendingContent = '';
      this.error.set(null);
      this.signalr.sendMessage(this.activeConversationId()!, lastUserMsg.content);
    }
  }

  clearConversation(): void {
    this.activeConversationId.set(null);
    this.activeConversation.set(null);
    this.messages.set([]);
    this.streamingContent.set('');
    this.pendingContent = '';
    this.error.set(null);
  }

  private handleChunk(chunk: AiStreamChunk): void {
    if (chunk.isComplete) {
      const fullContent = this.pendingContent;
      this.isStreaming.set(false);
      this.streamingContent.set('');

      const assistantMsg: AiMessage = {
        id: chunk.messageId ?? 0,
        conversationId: chunk.conversationId,
        role: 'Assistant',
        content: fullContent,
        sources: chunk.sources ?? [],
        createdAt: new Date().toISOString(),
      };
      this.messages.update((msgs) => [...msgs, assistantMsg]);
      this.pendingContent = '';

      if (!this.activeConversationId()) {
        this.loadConversations();
      }
    } else if (chunk.error) {
      this.error.set(chunk.error);
      this.isStreaming.set(false);
      this.pendingContent = '';
    } else {
      this.pendingContent += chunk.content;
      this.streamingContent.set(this.pendingContent);
    }
  }

  private handleNewConversation(conv: AiConversationSummary): void {
    this.conversations.update((convs) => [conv, ...convs]);
    this.activeConversationId.set(conv.id);
    this.activeConversation.set(conv);
    this.messages.set([]);
  }
}
