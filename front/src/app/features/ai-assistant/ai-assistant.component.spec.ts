import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { signal } from '@angular/core';
import AiAssistantComponent from './ai-assistant.component';
import { AiStore } from './services/ai-store.service';

describe('AiAssistantComponent', () => {
  let mockStore: Record<string, any>;

  beforeEach(() => {
    mockStore = {
      isOpen: signal(false),
      init: vi.fn(),
      destroy: vi.fn(),
      activeConversationId: signal<number | null>(null),
      isStreaming: signal(false),
      messages: signal([]),
      streamingContent: signal(''),
      error: signal<string | null>(null),
      suggestedPrompts: signal<string[]>([]),
      searchQuery: signal(''),
      conversations: signal([]),
      filteredConversations: signal([]),
      hasMoreConversations: signal(false),
      conversationsLoading: signal(false),
      conversationsPage: signal(1),
      clearConversation: vi.fn(),
      close: vi.fn(),
      sendMessage: vi.fn(),
      startNewConversation: vi.fn(),
      retryLastMessage: vi.fn(),
      selectConversation: vi.fn(),
      loadConversations: vi.fn(),
      openDashboard: vi.fn(),
    };

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [AiAssistantComponent],
      providers: [{ provide: AiStore, useValue: mockStore }],
      schemas: [NO_ERRORS_SCHEMA],
    });
  });

  function createComponent() {
    const comp = TestBed.createComponent(AiAssistantComponent);
    comp.detectChanges();
    return comp;
  }

  it('should create', () => {
    const comp = createComponent();
    expect(comp.componentInstance).toBeTruthy();
  });

  it('should call store.init on init', () => {
    const comp = createComponent();
    expect(mockStore.init).toHaveBeenCalledOnce();
  });

  it('should call store.destroy on destroy', () => {
    const comp = createComponent();
    comp.componentInstance.ngOnDestroy();
    expect(mockStore.destroy).toHaveBeenCalledOnce();
  });

  it('should render ai-button', () => {
    const comp = createComponent();
    const button = comp.nativeElement.querySelector('app-ai-button');
    expect(button).toBeTruthy();
  });

  it('should NOT render ai-chat-window when closed', () => {
    mockStore.isOpen.set(false);
    const comp = createComponent();
    const chatWindow = comp.nativeElement.querySelector('app-ai-chat-window');
    expect(chatWindow).toBeFalsy();
  });

  it('should render ai-chat-window when open', () => {
    mockStore.isOpen.set(true);
    const comp = createComponent();
    const chatWindow = comp.nativeElement.querySelector('app-ai-chat-window');
    expect(chatWindow).toBeTruthy();
  });
});
