import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { AiStore } from './ai-store.service';
import { AiApiService } from './ai-api.service';
import { AiSignalrService } from './ai-signalr.service';
import { AuthService } from '../../../core/services/auth.service';
import { SessionService } from './session.service';
import { Subject, of } from 'rxjs';
import { signal, computed } from '@angular/core';
import {
  AiConversationSummary,
  AiConversationDetail,
  AiMessage,
  AiStreamChunk,
} from '../models/ai.models';

function createConv(overrides: Partial<AiConversationSummary> = {}): AiConversationSummary {
  return {
    id: 1,
    userId: 1,
    sessionId: null,
    userRole: 'Admin',
    title: 'Test',
    language: 'en',
    isArchived: false,
    isHidden: false,
    lastMessage: null,
    messageCount: 0,
    createdAt: new Date().toISOString(),
    updatedAt: null,
    ...overrides,
  };
}

function createMsg(overrides: Partial<AiMessage> = {}): AiMessage {
  return {
    id: 0,
    conversationId: 1,
    role: 'User',
    content: 'Hello',
    sources: [],
    createdAt: new Date().toISOString(),
    ...overrides,
  };
}

describe('AiStore', () => {
  let store: AiStore;
  let mockApi: {
    getConversations: ReturnType<typeof vi.fn>;
    getConversation: ReturnType<typeof vi.fn>;
    startConversation: ReturnType<typeof vi.fn>;
    deleteConversation: ReturnType<typeof vi.fn>;
    searchConversations: ReturnType<typeof vi.fn>;
    sendMessage: ReturnType<typeof vi.fn>;
    getMessages: ReturnType<typeof vi.fn>;
    getSuggestions: ReturnType<typeof vi.fn>;
  };
  let mockSignalr: {
    onChunk$: Subject<AiStreamChunk>;
    onError$: Subject<string>;
    onConversationCreated$: Subject<AiConversationSummary>;
    startConnection: ReturnType<typeof vi.fn>;
    stopConnection: ReturnType<typeof vi.fn>;
    sendMessage: ReturnType<typeof vi.fn>;
  };
  let mockAuth: {
    user: ReturnType<typeof signal>;
    isAuthenticated: ReturnType<typeof computed>;
    isAdmin: ReturnType<typeof computed>;
    isWorker: ReturnType<typeof computed>;
    isCustomer: ReturnType<typeof computed>;
    getAccessToken: ReturnType<typeof vi.fn>;
  };
  let mockSession: {
    getSessionId: ReturnType<typeof vi.fn>;
    newSessionId: ReturnType<typeof vi.fn>;
    clearSessionId: ReturnType<typeof vi.fn>;
  };

  function guestAuth() {
    mockAuth.user.set(null);
  }

  function adminAuth() {
    mockAuth.user.set({ id: 1, firstName: '', lastName: '', email: '', phoneNumber: null, roles: ['Admin'] });
  }

  function workerAuth() {
    mockAuth.user.set({ id: 2, firstName: '', lastName: '', email: '', phoneNumber: null, roles: ['Worker'] });
  }

  function customerAuth() {
    mockAuth.user.set({ id: 3, firstName: '', lastName: '', email: '', phoneNumber: null, roles: ['Customer'] });
  }

  beforeEach(() => {
    mockApi = {
      getConversations: vi.fn(),
      getConversation: vi.fn(),
      startConversation: vi.fn(),
      deleteConversation: vi.fn(),
      searchConversations: vi.fn(),
      sendMessage: vi.fn(),
      getMessages: vi.fn(),
      getSuggestions: vi.fn(),
    };

    const chunkSub = new Subject<AiStreamChunk>();
    const errSub = new Subject<string>();
    const convSub = new Subject<AiConversationSummary>();
    mockSignalr = {
      onChunk$: chunkSub,
      onError$: errSub,
      onConversationCreated$: convSub,
      startConnection: vi.fn(),
      stopConnection: vi.fn(),
      sendMessage: vi.fn(),
    };

    const userSig = signal<any>(null);
    const rolesSig = computed(() => userSig()?.roles ?? []);
    mockAuth = {
      user: userSig,
      isAuthenticated: computed(() => userSig() !== null),
      isAdmin: computed(() => rolesSig().includes('Admin')),
      isWorker: computed(() => rolesSig().includes('Worker')),
      isCustomer: computed(() => rolesSig().includes('Customer')),
      getAccessToken: vi.fn(),
    };

    mockSession = {
      getSessionId: vi.fn().mockReturnValue('sess-123'),
      newSessionId: vi.fn(),
      clearSessionId: vi.fn(),
    };

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        AiStore,
        { provide: AiApiService, useValue: mockApi },
        { provide: AiSignalrService, useValue: mockSignalr },
        { provide: AuthService, useValue: mockAuth },
        { provide: SessionService, useValue: mockSession },
      ],
    });

    store = TestBed.inject(AiStore);
  });

  afterEach(() => {
    store.destroy();
  });

  // ─── Initial state ─────────────────────────────────
  describe('initial state', () => {
    it('should have default signal values', () => {
      expect(store.isOpen()).toBe(false);
      expect(store.isDashboardMode()).toBe(false);
      expect(store.conversations()).toEqual([]);
      expect(store.conversationsPage()).toBe(1);
      expect(store.hasMoreConversations()).toBe(true);
      expect(store.conversationsLoading()).toBe(false);
      expect(store.activeConversationId()).toBeNull();
      expect(store.activeConversation()).toBeNull();
      expect(store.messages()).toEqual([]);
      expect(store.messagesLoading()).toBe(false);
      expect(store.isStreaming()).toBe(false);
      expect(store.streamingContent()).toBe('');
      expect(store.error()).toBeNull();
      expect(store.suggestedPrompts()).toEqual([]);
      expect(store.searchQuery()).toBe('');
    });
  });

  // ─── Computed signals ──────────────────────────────
  describe('currentUserRole computed', () => {
    it('should return Guest when not authenticated', () => {
      guestAuth();
      expect(store.currentUserRole()).toBe('Guest');
    });

    it('should return Admin for admin users', () => {
      adminAuth();
      expect(store.currentUserRole()).toBe('Admin');
    });

    it('should return Worker for workers', () => {
      workerAuth();
      expect(store.currentUserRole()).toBe('Worker');
    });

    it('should return Customer for customers', () => {
      customerAuth();
      expect(store.currentUserRole()).toBe('Customer');
    });
  });

  describe('isAuthenticated computed', () => {
    it('should be false when no user', () => {
      guestAuth();
      expect(store.isAuthenticated()).toBe(false);
    });

    it('should be true when user is set', () => {
      adminAuth();
      expect(store.isAuthenticated()).toBe(true);
    });
  });

  describe('filteredConversations computed', () => {
    it('should return all conversations when searchQuery is empty', () => {
      const convs = [createConv({ title: 'Hello' }), createConv({ title: 'World' })];
      store.conversations.set(convs);
      store.searchQuery.set('');
      expect(store.filteredConversations()).toEqual(convs);
    });

    it('should filter by search query', () => {
      store.conversations.set([createConv({ title: 'Plumbing' }), createConv({ title: 'Electrical' })]);
      store.searchQuery.set('plumb');
      expect(store.filteredConversations()).toHaveLength(1);
      expect(store.filteredConversations()[0].title).toBe('Plumbing');
    });

    it('should be case-insensitive', () => {
      store.conversations.set([createConv({ title: 'Plumbing' }), createConv({ title: 'Electrical' })]);
      store.searchQuery.set('PLUMB');
      expect(store.filteredConversations()).toHaveLength(1);
    });
  });

  // ─── toggle / open / close ─────────────────────────
  describe('toggle', () => {
    it('should open widget mode when closed', () => {
      guestAuth();
      store.toggle();
      expect(store.isOpen()).toBe(true);
      expect(store.isDashboardMode()).toBe(false);
    });

    it('should close when already open', () => {
      store.isOpen.set(true);
      store.toggle();
      expect(store.isOpen()).toBe(false);
    });

    it('should rotate guest session when opening as guest', () => {
      guestAuth();
      store.toggle();
      expect(mockSession.newSessionId).toHaveBeenCalledOnce();
    });

    it('should reset conversation state for guest opening', () => {
      guestAuth();
      store.conversations.set([createConv()]);
      store.hasMoreConversations.set(false);
      store.conversationsPage.set(3);
      store.toggle();
      expect(store.conversations()).toEqual([]);
      expect(store.hasMoreConversations()).toBe(true);
      expect(store.conversationsPage()).toBe(1);
    });

    it('should NOT rotate session for authenticated user', () => {
      adminAuth();
      store.toggle();
      expect(mockSession.newSessionId).not.toHaveBeenCalled();
    });

    it('should load suggestions on open', () => {
      guestAuth();
      mockApi.getSuggestions.mockReturnValue(of({ prompts: ['Help'] }));
      store.toggle();
      expect(mockApi.getSuggestions).toHaveBeenCalledOnce();
    });

    it('should clear active conversation on open', () => {
      guestAuth();
      store.activeConversationId.set(5);
      store.activeConversation.set(createConv());
      store.messages.set([createMsg()]);
      store.toggle();
      expect(store.activeConversationId()).toBeNull();
      expect(store.messages()).toEqual([]);
    });
  });

  describe('open', () => {
    it('should open widget mode', () => {
      guestAuth();
      store.open();
      expect(store.isOpen()).toBe(true);
      expect(store.isDashboardMode()).toBe(false);
    });

    it('should not toggle if already open', () => {
      store.isOpen.set(true);
      store.open();
      expect(store.isOpen()).toBe(true);
    });

    it('should rotate guest session for unauthenticated', () => {
      guestAuth();
      store.open();
      expect(mockSession.newSessionId).toHaveBeenCalledOnce();
    });
  });

  describe('openDashboard', () => {
    it('should open in dashboard mode', () => {
      mockApi.getConversations.mockReturnValue(of({ items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0, hasNextPage: false, hasPreviousPage: false }));
      mockApi.getSuggestions.mockReturnValue(of({ prompts: [] }));
      store.openDashboard();
      expect(store.isOpen()).toBe(true);
      expect(store.isDashboardMode()).toBe(true);
    });

    it('should load conversations', async () => {
      const paged = { items: [createConv()], page: 1, pageSize: 20, totalCount: 1, totalPages: 1, hasNextPage: false, hasPreviousPage: false };
      mockApi.getConversations.mockReturnValue(of(paged));
      mockApi.getSuggestions.mockReturnValue(of({ prompts: [] }));
      store.openDashboard();
      await new Promise(r => setTimeout(r, 0));
      expect(mockApi.getConversations).toHaveBeenCalledWith(1, 20);
      expect(store.conversations()).toEqual(paged.items);
    });

    it('should load suggestions', async () => {
      mockApi.getConversations.mockReturnValue(of({ items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0, hasNextPage: false, hasPreviousPage: false }));
      mockApi.getSuggestions.mockReturnValue(of({ prompts: ['Ask me'] }));
      store.openDashboard();
      await new Promise(r => setTimeout(r, 0));
      expect(store.suggestedPrompts()).toEqual(['Ask me']);
    });
  });

  describe('close', () => {
    it('should close both modes', () => {
      store.isOpen.set(true);
      store.isDashboardMode.set(true);
      store.close();
      expect(store.isOpen()).toBe(false);
      expect(store.isDashboardMode()).toBe(false);
    });
  });

  // ─── init / destroy ────────────────────────────────
  describe('init', () => {
    it('should start signalr connection', () => {
      store.init();
      expect(mockSignalr.startConnection).toHaveBeenCalledOnce();
    });

    it('should subscribe to onChunk$', () => {
      store.init();
      const chunk: AiStreamChunk = { conversationId: 1, content: 'Hi', isComplete: false };
      mockSignalr.onChunk$.next(chunk);
      expect(store.streamingContent()).toBe('Hi');
    });

    it('should subscribe to onError$', () => {
      store.init();
      mockSignalr.onError$.next('Error!');
      expect(store.error()).toBe('Error!');
      expect(store.isStreaming()).toBe(false);
    });

    it('should subscribe to onConversationCreated$', () => {
      store.init();
      const conv = createConv({ id: 99, title: 'New' });
      mockSignalr.onConversationCreated$.next(conv);
      expect(store.conversations()).toContainEqual(conv);
      expect(store.activeConversationId()).toBe(99);
    });
  });

  describe('destroy', () => {
    it('should stop signalr connection', () => {
      store.init();
      store.destroy();
      expect(mockSignalr.stopConnection).toHaveBeenCalledOnce();
    });

    it('should unsubscribe from signalr events', () => {
      store.init();
      store.destroy();
      // After destroy, chunks should not update state
      const chunk: AiStreamChunk = { conversationId: 1, content: 'Should not appear', isComplete: false };
      mockSignalr.onChunk$.next(chunk);
      expect(store.streamingContent()).toBe('');
    });
  });

  // ─── Conversation management ───────────────────────
  describe('loadConversations', () => {
    it('should load first page and replace items', async () => {
      const convs = [createConv({ id: 1 })];
      const paged = { items: convs, page: 1, pageSize: 20, totalCount: 1, totalPages: 1, hasNextPage: false, hasPreviousPage: false };
      mockApi.getConversations.mockReturnValue(of(paged));
      store.conversations.set([createConv({ id: 99 })]);
      await store.loadConversations(1);
      expect(store.conversations()).toEqual(convs);
      expect(store.hasMoreConversations()).toBe(false);
    });

    it('should append subsequent pages', async () => {
      const page1 = [createConv({ id: 1 })];
      const page2 = [createConv({ id: 2 })];
      mockApi.getConversations.mockReturnValue(of({ items: page1, page: 1, pageSize: 20, totalCount: 2, totalPages: 2, hasNextPage: true, hasPreviousPage: false }));
      await store.loadConversations(1);
      mockApi.getConversations.mockReturnValue(of({ items: page2, page: 2, pageSize: 20, totalCount: 2, totalPages: 2, hasNextPage: false, hasPreviousPage: true }));
      await store.loadConversations(2);
      expect(store.conversations()).toEqual([...page1, ...page2]);
    });

    it('should not load when already loading', async () => {
      store.conversationsLoading.set(true);
      await store.loadConversations(1);
      expect(mockApi.getConversations).not.toHaveBeenCalled();
    });

    it('should not load when hasMoreConversations is false', async () => {
      store.hasMoreConversations.set(false);
      await store.loadConversations(1);
      expect(mockApi.getConversations).not.toHaveBeenCalled();
    });

    it('should increment page number', async () => {
      mockApi.getConversations.mockReturnValue(of({ items: [createConv()], page: 1, pageSize: 20, totalCount: 1, totalPages: 1, hasNextPage: true, hasPreviousPage: false }));
      expect(store.conversationsPage()).toBe(1);
      await store.loadConversations(1);
      expect(store.conversationsPage()).toBe(2);
    });
  });

  describe('loadSuggestions', () => {
    it('should set prompts from API', async () => {
      mockApi.getSuggestions.mockReturnValue(of({ prompts: ['Ask', 'Help'] }));
      await store.loadSuggestions();
      expect(store.suggestedPrompts()).toEqual(['Ask', 'Help']);
    });

    it('should silently handle errors', async () => {
      mockApi.getSuggestions.mockRejectedValue(new Error('fail'));
      await expect(store.loadSuggestions()).resolves.toBeUndefined();
    });
  });

  describe('selectConversation', () => {
    it('should set active conversation and load messages', async () => {
      const conv = createConv({ id: 5 });
      store.conversations.set([conv]);
      const detail: AiConversationDetail = { ...conv, messages: [createMsg({ id: 1, conversationId: 5 })] };
      mockApi.getConversation.mockReturnValue(of(detail));
      await store.selectConversation(5);
      expect(store.activeConversationId()).toBe(5);
      expect(store.activeConversation()).toEqual(conv);
      expect(store.messages()).toEqual(detail.messages);
    });

    it('should set null activeConversation if not in list', async () => {
      const detail: AiConversationDetail = { ...createConv({ id: 5 }), messages: [] };
      mockApi.getConversation.mockReturnValue(of(detail));
      await store.selectConversation(5);
      expect(store.activeConversation()).toBeNull();
    });

    it('should set error on failure', async () => {
      store.conversations.set([createConv({ id: 5 })]);
      mockApi.getConversation.mockRejectedValue(new Error('fail'));
      await store.selectConversation(5);
      expect(store.error()).toBe('Failed to load conversation');
    });

    it('should clear previous error', async () => {
      store.error.set('old error');
      store.conversations.set([createConv({ id: 5 })]);
      mockApi.getConversation.mockRejectedValue(new Error('fail'));
      await store.selectConversation(5);
      expect(store.error()).not.toBe('old error');
    });
  });

  // ─── Messaging ─────────────────────────────────────
  describe('sendMessage', () => {
    beforeEach(() => {
      store.activeConversationId.set(1);
      store.activeConversation.set(createConv());
    });

    it('should return early if no active conversation', async () => {
      store.activeConversationId.set(null);
      await store.sendMessage('Hello');
      expect(mockSignalr.sendMessage).not.toHaveBeenCalled();
    });

    it('should return early if content is empty', async () => {
      await store.sendMessage('   ');
      expect(mockSignalr.sendMessage).not.toHaveBeenCalled();
    });

    it('should return early if already streaming', async () => {
      store.isStreaming.set(true);
      await store.sendMessage('Hello');
      expect(mockSignalr.sendMessage).not.toHaveBeenCalled();
    });

    it('should append user message to messages list', async () => {
      store.messages.set([createMsg({ id: 1, role: 'Assistant' })]);
      mockSignalr.sendMessage.mockResolvedValue(undefined);
      await store.sendMessage('User text');
      expect(store.messages()).toHaveLength(2);
      expect(store.messages()[1].role).toBe('User');
      expect(store.messages()[1].content).toBe('User text');
    });

    it('should set isStreaming to true', async () => {
      mockSignalr.sendMessage.mockResolvedValue(undefined);
      await store.sendMessage('Hello');
      expect(store.isStreaming()).toBe(true);
    });

    it('should call signalr.sendMessage', async () => {
      mockSignalr.sendMessage.mockResolvedValue(undefined);
      await store.sendMessage('Hello');
      expect(mockSignalr.sendMessage).toHaveBeenCalledWith(1, 'Hello');
    });
  });

  describe('startNewConversation', () => {
    it('should call API and handle new conversation', async () => {
      const conv = createConv({ id: 10 });
      mockApi.startConversation.mockReturnValue(of(conv));
      await store.startNewConversation();
      expect(store.conversations()).toContainEqual(conv);
      expect(store.activeConversationId()).toBe(10);
    });

    it('should send first message after creation', async () => {
      vi.useFakeTimers();
      try {
        const conv = createConv({ id: 10 });
        mockApi.startConversation.mockReturnValue(of(conv));
        mockSignalr.sendMessage.mockResolvedValue(undefined);
        store.startNewConversation('First msg');
        await vi.advanceTimersByTimeAsync(150);
        expect(mockSignalr.sendMessage).toHaveBeenCalledWith(10, 'First msg');
      } finally {
        vi.useRealTimers();
      }
    });

    it('should set error and reset on API failure', async () => {
      mockApi.startConversation.mockRejectedValue(new Error('fail'));
      await store.startNewConversation();
      expect(store.error()).toBe('Failed to start conversation');
      expect(store.activeConversationId()).toBeNull();
      expect(store.messages()).toEqual([]);
    });
  });

  describe('deleteConversation', () => {
    it('should remove conversation from list', async () => {
      const convs = [createConv({ id: 1 }), createConv({ id: 2 })];
      store.conversations.set(convs);
      mockApi.deleteConversation.mockReturnValue(of(true));
      await store.deleteConversation(1);
      expect(store.conversations()).toHaveLength(1);
      expect(store.conversations()[0].id).toBe(2);
    });

    it('should clear active conversation if deleted', async () => {
      store.conversations.set([createConv({ id: 1 })]);
      store.activeConversationId.set(1);
      store.activeConversation.set(createConv());
      store.messages.set([createMsg()]);
      mockApi.deleteConversation.mockReturnValue(of(true));
      await store.deleteConversation(1);
      expect(store.activeConversationId()).toBeNull();
      expect(store.activeConversation()).toBeNull();
      expect(store.messages()).toEqual([]);
    });

    it('should not clear active if different conversation deleted', async () => {
      store.conversations.set([createConv({ id: 1 }), createConv({ id: 2 })]);
      store.activeConversationId.set(1);
      mockApi.deleteConversation.mockReturnValue(of(true));
      await store.deleteConversation(2);
      expect(store.activeConversationId()).toBe(1);
    });

    it('should silently handle errors', async () => {
      store.conversations.set([createConv({ id: 1 })]);
      mockApi.deleteConversation.mockRejectedValue(new Error('fail'));
      await expect(store.deleteConversation(1)).resolves.toBeUndefined();
    });
  });

  describe('retryLastMessage', () => {
    beforeEach(() => {
      store.activeConversationId.set(1);
    });

    it('should resend last user message', () => {
      store.messages.set([
        createMsg({ id: 1, role: 'Assistant', content: 'Response' }),
        createMsg({ id: 0, role: 'User', content: 'Retry this' }),
      ]);
      store.retryLastMessage();
      expect(mockSignalr.sendMessage).toHaveBeenCalledWith(1, 'Retry this');
    });

    it('should remove pending assistant messages', () => {
      store.messages.set([
        createMsg({ id: 0, role: 'User', content: 'Hi' }),
        createMsg({ id: 0, role: 'Assistant', content: 'Thinking...' }),
      ]);
      store.retryLastMessage();
      expect(store.messages()).toHaveLength(1);
      expect(store.messages()[0].role).toBe('User');
    });

    it('should set streaming state', () => {
      store.messages.set([createMsg({ role: 'User', content: 'Hi' })]);
      store.retryLastMessage();
      expect(store.isStreaming()).toBe(true);
      expect(store.streamingContent()).toBe('');
      expect(store.error()).toBeNull();
    });
  });

  describe('clearConversation', () => {
    it('should reset all active state', () => {
      store.activeConversationId.set(5);
      store.activeConversation.set(createConv());
      store.messages.set([createMsg()]);
      store.streamingContent.set('typing...');
      store.error.set('some error');
      store.clearConversation();
      expect(store.activeConversationId()).toBeNull();
      expect(store.activeConversation()).toBeNull();
      expect(store.messages()).toEqual([]);
      expect(store.streamingContent()).toBe('');
      expect(store.error()).toBeNull();
    });
  });

  // ─── Streaming ────────────────────────────────────
  describe('streaming via signalr events', () => {
    beforeEach(() => {
      store.init();
      store.activeConversationId.set(1);
    });

    it('should accumulate content from incomplete chunks', () => {
      mockSignalr.onChunk$.next({ conversationId: 1, content: 'Hel', isComplete: false });
      mockSignalr.onChunk$.next({ conversationId: 1, content: 'lo', isComplete: false });
      expect(store.streamingContent()).toBe('Hello');
    });

    it('should add assistant message on complete chunk', () => {
      mockSignalr.onChunk$.next({ conversationId: 1, content: 'Done', isComplete: false });
      mockSignalr.onChunk$.next({ conversationId: 1, content: '', messageId: 42, sources: [], isComplete: true });
      expect(store.isStreaming()).toBe(false);
      expect(store.streamingContent()).toBe('');
      expect(store.messages()).toHaveLength(1);
      expect(store.messages()[0].role).toBe('Assistant');
      expect(store.messages()[0].content).toBe('Done');
      expect(store.messages()[0].id).toBe(42);
    });

    it('should set error on error chunk', () => {
      mockSignalr.onChunk$.next({ conversationId: 1, content: '', error: 'Something failed', isComplete: false });
      expect(store.error()).toBe('Something failed');
      expect(store.isStreaming()).toBe(false);
    });

    it('should reload conversations if no active conversation on complete', () => {
      store.activeConversationId.set(null);
      mockApi.getConversations.mockReturnValue(of({ items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0, hasNextPage: false, hasPreviousPage: false }));
      mockSignalr.onChunk$.next({ conversationId: 1, content: '', messageId: 10, sources: [], isComplete: true });
      expect(mockApi.getConversations).toHaveBeenCalled();
    });
  });

  describe('streaming timeout', () => {
    it('should set error after 30 seconds', async () => {
      vi.useFakeTimers();
      try {
        store.activeConversationId.set(1);
        store.activeConversation.set(createConv());
        mockSignalr.sendMessage.mockResolvedValue(undefined);
        await store.sendMessage('Hello');
        await vi.advanceTimersByTimeAsync(30000);
        expect(store.isStreaming()).toBe(false);
        expect(store.error()).toBe('Response timed out. Please try again.');
      } finally {
        vi.useRealTimers();
      }
    });

    it('should clear timeout when chunk completes', () => {
      vi.useFakeTimers();
      try {
        store.activeConversationId.set(1);
        store.init();
        // Start streaming
        mockSignalr.onChunk$.next({ conversationId: 1, content: '', messageId: 5, sources: [], isComplete: true });
        // Advance past 30s - timeout should have been cleared
        vi.advanceTimersByTime(31000);
        expect(store.error()).not.toBe('Response timed out. Please try again.');
      } finally {
        vi.useRealTimers();
      }
    });
  });

  // ─── handleNewConversation ─────────────────────────
  describe('conversation created via signalr', () => {
    beforeEach(() => {
      store.init();
    });

    it('should prepend conversation to list', () => {
      const existing = createConv({ id: 1 });
      store.conversations.set([existing]);
      const newConv = createConv({ id: 2, title: 'New' });
      mockSignalr.onConversationCreated$.next(newConv);
      expect(store.conversations()).toHaveLength(2);
      expect(store.conversations()[0].title).toBe('New');
    });

    it('should set as active conversation', () => {
      const conv = createConv({ id: 7, title: 'Active' });
      mockSignalr.onConversationCreated$.next(conv);
      expect(store.activeConversationId()).toBe(7);
      expect(store.activeConversation()).toEqual(conv);
    });
  });
});
