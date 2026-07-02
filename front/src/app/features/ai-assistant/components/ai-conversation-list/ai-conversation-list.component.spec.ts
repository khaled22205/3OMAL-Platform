import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { signal, computed } from '@angular/core';
import { AiConversationListComponent } from './ai-conversation-list.component';
import { AiStore } from '../../services/ai-store.service';
import { AiConversationSummary } from '../../models/ai.models';

function makeConv(id: number, title: string): AiConversationSummary {
  return {
    id,
    userId: 1,
    sessionId: null,
    userRole: 'Admin',
    title,
    language: 'en',
    isArchived: false,
    isHidden: false,
    lastMessage: null,
    messageCount: 3,
    createdAt: new Date().toISOString(),
    updatedAt: null,
  };
}

describe('AiConversationListComponent', () => {
  let mockStore: Record<string, any>;
  let convs: AiConversationSummary[];

  beforeEach(() => {
    convs = [makeConv(1, 'Plumbing'), makeConv(2, 'Electrical')];
    const sq = signal('');
    const c = signal(convs);
    const filtered = computed(() => {
      const q = sq().toLowerCase();
      if (!q) return c();
      return c().filter((x) => x.title.toLowerCase().includes(q));
    });

    mockStore = {
      searchQuery: sq,
      conversations: c,
      filteredConversations: filtered,
      activeConversationId: signal<number | null>(null),
      hasMoreConversations: signal(false),
      conversationsLoading: signal(false),
      conversationsPage: signal(1),
      startNewConversation: vi.fn(),
      selectConversation: vi.fn(),
      loadConversations: vi.fn(),
    };

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [AiConversationListComponent],
      providers: [{ provide: AiStore, useValue: mockStore }],
    });
  });

  function createComponent() {
    const comp = TestBed.createComponent(AiConversationListComponent);
    comp.detectChanges();
    return comp;
  }

  it('should render search input', () => {
    const comp = createComponent();
    const input = comp.nativeElement.querySelector('input[placeholder="Search conversations..."]');
    expect(input).toBeTruthy();
  });

  it('should render new conversation button', () => {
    const comp = createComponent();
    const btn = comp.nativeElement.querySelector('button');
    expect(btn?.textContent).toContain('New conversation');
  });

  it('should call startNewConversation on new conv button click', () => {
    const comp = createComponent();
    const buttons = comp.nativeElement.querySelectorAll('button');
    const newBtn = Array.from(buttons).find((b: Element) => b.textContent?.includes('New'));
    (newBtn as HTMLElement)?.click();
    expect(mockStore.startNewConversation).toHaveBeenCalledOnce();
  });

  it('should render conversation items', () => {
    const comp = createComponent();
    const text = comp.nativeElement.textContent;
    expect(text).toContain('Plumbing');
    expect(text).toContain('Electrical');
  });

  it('should show message count', () => {
    const comp = createComponent();
    expect(comp.nativeElement.textContent).toContain('3 messages');
  });

  it('should call selectConversation on item click', () => {
    const comp = createComponent();
    const items = comp.nativeElement.querySelectorAll('button');
    const convBtn = Array.from(items).find(
      (b: Element) => b.textContent?.includes('Plumbing') && !b.textContent?.includes('New'),
    );
    (convBtn as HTMLElement)?.click();
    expect(mockStore.selectConversation).toHaveBeenCalledWith(1);
  });

  it('should filter conversations by search', () => {
    mockStore.searchQuery.set('elect');
    const comp = createComponent();
    const text = comp.nativeElement.textContent;
    expect(text).toContain('Electrical');
    expect(text).not.toContain('Plumbing');
  });

  it('should show empty state when no conversations', () => {
    mockStore.conversations.set([]);
    const comp = createComponent();
    expect(comp.nativeElement.textContent).toContain('No conversations yet');
  });

  it('should show load more button when hasMoreConversations', () => {
    mockStore.hasMoreConversations.set(true);
    const comp = createComponent();
    expect(comp.nativeElement.textContent).toContain('Load more');
  });

  it('should call loadConversations on load more click', () => {
    mockStore.hasMoreConversations.set(true);
    const comp = createComponent();
    const loadMore = Array.from(comp.nativeElement.querySelectorAll('button')).find(
      (b: Element) => b.textContent?.trim() === 'Load more',
    );
    expect(loadMore).toBeTruthy();
    (loadMore as HTMLElement)?.click();
    expect(mockStore.loadConversations).toHaveBeenCalledWith(1);
  });

  it('should show loading state during conversation load', () => {
    mockStore.hasMoreConversations.set(true);
    mockStore.conversationsLoading.set(true);
    const comp = createComponent();
    expect(comp.nativeElement.textContent).toContain('Loading...');
  });
});
