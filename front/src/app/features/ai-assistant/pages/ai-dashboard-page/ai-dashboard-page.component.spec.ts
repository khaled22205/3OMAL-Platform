import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { Component, signal, input, output } from '@angular/core';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import AiDashboardPageComponent from './ai-dashboard-page.component';
import { AiStore } from '../../services/ai-store.service';
import { AiMessage } from '../../models/ai.models';
import { AiMessageBubbleComponent } from '../../components/ai-message-bubble/ai-message-bubble.component';

@Component({
  standalone: true,
  selector: 'app-ai-message-bubble',
  template: '',
})
class MockMessageBubble {
  readonly message = input<AiMessage>();
  readonly copy = output<AiMessage>();
  readonly retry = output<void>();
}

function makeMsg(overrides: Partial<AiMessage> = {}): AiMessage {
  return {
    id: 1,
    conversationId: 1,
    role: 'User',
    content: 'Hi',
    sources: [],
    createdAt: new Date().toISOString(),
    ...overrides,
  };
}

function createMockStore() {
  return {
    activeConversationId: signal<number | null>(null),
    isStreaming: signal(false),
    messages: signal<AiMessage[]>([]),
    streamingContent: signal(''),
    error: signal<string | null>(null),
    suggestedPrompts: signal<string[]>([]),
    searchQuery: signal(''),
    conversations: signal([]),
    filteredConversations: signal([]),
    hasMoreConversations: signal(false),
    conversationsLoading: signal(false),
    conversationsPage: signal(1),
    openDashboard: vi.fn(),
    clearConversation: vi.fn(),
    close: vi.fn(),
    sendMessage: vi.fn(),
    startNewConversation: vi.fn(),
    retryLastMessage: vi.fn(),
    selectConversation: vi.fn(),
    loadConversations: vi.fn(),
  };
}

describe('AiDashboardPageComponent', () => {
  let mockStore: ReturnType<typeof createMockStore>;

  beforeEach(() => {
    mockStore = createMockStore();
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [AiDashboardPageComponent],
      providers: [{ provide: AiStore, useValue: mockStore }],
      schemas: [NO_ERRORS_SCHEMA],
    });
    TestBed.overrideComponent(AiDashboardPageComponent, {
      remove: { imports: [AiMessageBubbleComponent] },
      add: { imports: [MockMessageBubble] },
    });
  });

  function createComponent() {
    const comp = TestBed.createComponent(AiDashboardPageComponent);
    comp.detectChanges();
    return comp;
  }

  it('should create', () => {
    const comp = createComponent();
    expect(comp.componentInstance).toBeTruthy();
  });

  it('should call openDashboard on init', () => {
    const comp = createComponent();
    expect(mockStore.openDashboard).toHaveBeenCalledOnce();
  });

  it('should render AI Assistant header', () => {
    const comp = createComponent();
    expect(comp.nativeElement.textContent).toContain('AI Assistant');
  });

  it('should render new conversation button', () => {
    const comp = createComponent();
    const btn = Array.from(comp.nativeElement.querySelectorAll('button')).find(
      (b: Element) => b.textContent?.includes('New conversation'),
    );
    expect(btn).toBeTruthy();
  });

  it('should call startNewConversation on new chat button click', () => {
    const comp = createComponent();
    const btn = Array.from(comp.nativeElement.querySelectorAll('button')).find(
      (b: Element) => b.textContent?.includes('New conversation'),
    );
    (btn as HTMLElement)?.click();
    expect(mockStore.startNewConversation).toHaveBeenCalled();
  });

  it('should render conversation list', () => {
    const comp = createComponent();
    expect(comp.nativeElement.querySelector('app-ai-conversation-list')).toBeTruthy();
  });

  it('should show empty state when no active conversation', () => {
    mockStore.activeConversationId.set(null);
    const comp = createComponent();
    expect(comp.nativeElement.textContent).toContain('Select a conversation');
  });

  it('should show chat area when active conversation exists', () => {
    mockStore.activeConversationId.set(1);
    const comp = createComponent();
    expect(comp.nativeElement.querySelector('input')).toBeTruthy();
  });

  it('should render message bubble for each message', () => {
    mockStore.activeConversationId.set(1);
    mockStore.messages.set([makeMsg({ content: 'Dashboard msg' })]);
    const comp = createComponent();
    const bubbles = comp.nativeElement.querySelectorAll('app-ai-message-bubble');
    expect(bubbles.length).toBe(1);
    expect(comp.nativeElement.textContent).not.toContain('Select a conversation');
  });

  it('should show streaming indicator when streaming', () => {
    mockStore.activeConversationId.set(1);
    mockStore.isStreaming.set(true);
    mockStore.streamingContent.set('typing...');
    const comp = createComponent();
    expect(comp.nativeElement.textContent).toContain('typing...');
  });

  it('should show error with retry button', () => {
    mockStore.activeConversationId.set(1);
    mockStore.error.set('Dashboard error');
    const comp = createComponent();
    const retryBtn = Array.from(comp.nativeElement.querySelectorAll('button')).find(
      (b: Element) => b.textContent === 'Retry',
    );
    expect(retryBtn).toBeTruthy();
    expect(comp.nativeElement.textContent).toContain('Dashboard error');
  });

  it('should show suggested prompts when no messages and not streaming', () => {
    mockStore.activeConversationId.set(1);
    mockStore.suggestedPrompts.set(['Prompt 1']);
    const comp = createComponent();
    expect(comp.nativeElement.querySelector('app-ai-suggested-prompts')).toBeTruthy();
  });

  it('should send message', () => {
    mockStore.activeConversationId.set(1);
    const comp = createComponent();
    comp.componentInstance.inputText = 'Dashboard query';
    comp.componentInstance.sendMessage();
    expect(mockStore.sendMessage).toHaveBeenCalledWith('Dashboard query');
  });

  it('should start new conversation if no active conv on send', () => {
    mockStore.activeConversationId.set(null);
    const comp = createComponent();
    comp.componentInstance.inputText = 'New chat';
    comp.componentInstance.sendMessage();
    expect(mockStore.startNewConversation).toHaveBeenCalledWith('New chat');
  });

  it('should not send empty text', () => {
    mockStore.activeConversationId.set(1);
    const comp = createComponent();
    comp.componentInstance.inputText = '   ';
    comp.componentInstance.sendMessage();
    expect(mockStore.sendMessage).not.toHaveBeenCalled();
  });

  it('should not send while streaming', () => {
    mockStore.activeConversationId.set(1);
    mockStore.isStreaming.set(true);
    const comp = createComponent();
    comp.componentInstance.inputText = 'Hello';
    comp.componentInstance.sendMessage();
    expect(mockStore.sendMessage).not.toHaveBeenCalled();
  });

  it('should show input when streaming', () => {
    mockStore.activeConversationId.set(1);
    mockStore.isStreaming.set(true);
    const comp = createComponent();
    expect(comp.nativeElement.querySelector('input')).toBeTruthy();
    expect(comp.nativeElement.textContent).not.toContain('Select a conversation');
  });

  it('should handle suggestion click by sending message', () => {
    mockStore.activeConversationId.set(1);
    const comp = createComponent();
    comp.componentInstance.onSuggestionClick('Help');
    expect(comp.componentInstance.inputText).toBe('');
    expect(mockStore.sendMessage).toHaveBeenCalledWith('Help');
  });

  it('should copy message to clipboard', () => {
    const msg = makeMsg({ content: 'Copy dashboard' });
    const writeText = vi.fn().mockResolvedValue(undefined);
    Object.assign(navigator, { clipboard: { writeText } });
    const comp = createComponent();
    comp.componentInstance.copyMessage(msg);
    expect(writeText).toHaveBeenCalledWith('Copy dashboard');
  });
});
