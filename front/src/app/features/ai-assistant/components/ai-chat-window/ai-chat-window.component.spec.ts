import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { Component, signal, input, output } from '@angular/core';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { AiChatWindowComponent } from './ai-chat-window.component';
import { AiStore } from '../../services/ai-store.service';
import { AiMessage } from '../../models/ai.models';
import { AiMessageBubbleComponent } from '../ai-message-bubble/ai-message-bubble.component';

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
    clearConversation: vi.fn(),
    close: vi.fn(),
    sendMessage: vi.fn(),
    startNewConversation: vi.fn(),
    retryLastMessage: vi.fn(),
    selectConversation: vi.fn(),
    loadConversations: vi.fn(),
  };
}

describe('AiChatWindowComponent', () => {
  let mockStore: ReturnType<typeof createMockStore>;

  beforeEach(() => {
    mockStore = createMockStore();
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [AiChatWindowComponent],
      providers: [{ provide: AiStore, useValue: mockStore }],
      schemas: [NO_ERRORS_SCHEMA],
    });
    TestBed.overrideComponent(AiChatWindowComponent, {
      remove: { imports: [AiMessageBubbleComponent] },
      add: { imports: [MockMessageBubble] },
    });
  });

  function createComponent() {
    const comp = TestBed.createComponent(AiChatWindowComponent);
    comp.detectChanges();
    return comp;
  }

  it('should render AI header', () => {
    const comp = createComponent();
    expect(comp.nativeElement.textContent).toContain('3OMAL AI');
  });

  it('should show close button', () => {
    const comp = createComponent();
    const svgs = comp.nativeElement.querySelectorAll('svg');
    expect(svgs.length).toBeGreaterThan(0);
  });

  it('should show new conversation button when active conversation exists', () => {
    mockStore.activeConversationId.set(1);
    const comp = createComponent();
    const buttons = comp.nativeElement.querySelectorAll('button');
    const newBtn = Array.from(buttons).find((b: Element) =>
      b.innerHTML.includes('M12 4v16m8-8H4'),
    );
    expect(newBtn).toBeTruthy();
  });

  it('should call clearConversation on new conv button click', () => {
    mockStore.activeConversationId.set(1);
    const comp = createComponent();
    const buttons = comp.nativeElement.querySelectorAll('button');
    const newBtn = Array.from(buttons).find((b: Element) =>
      b.innerHTML.includes('M12 4v16m8-8H4'),
    );
    (newBtn as HTMLElement)?.click();
    expect(mockStore.clearConversation).toHaveBeenCalledOnce();
  });

  it('should call close on X button click', () => {
    const comp = createComponent();
    const closeBtn = Array.from(comp.nativeElement.querySelectorAll('button')).find(
      (b: Element) => b.innerHTML.includes('M6 18L18 6M6 6l12 12'),
    );
    (closeBtn as HTMLElement)?.click();
    expect(mockStore.close).toHaveBeenCalledOnce();
  });

  it('should show conversation list when no active conversation', () => {
    mockStore.activeConversationId.set(null);
    const comp = createComponent();
    expect(comp.nativeElement.querySelector('app-ai-conversation-list')).toBeTruthy();
  });

  it('should show chat area when active conversation exists', () => {
    mockStore.activeConversationId.set(1);
    const comp = createComponent();
    const input = comp.nativeElement.querySelector('input');
    expect(input).toBeTruthy();
  });

  it('should show empty state when no messages and not streaming', () => {
    mockStore.activeConversationId.set(1);
    const comp = createComponent();
    expect(comp.nativeElement.textContent).toContain('How can I help you today?');
  });

  it('should render message bubbles for each message', () => {
    mockStore.activeConversationId.set(1);
    mockStore.messages.set([makeMsg({ content: 'Test msg' })]);
    const comp = createComponent();
    const bubbles = comp.nativeElement.querySelectorAll('app-ai-message-bubble');
    expect(bubbles.length).toBe(1);
    // The mock bubble doesn't render content, but the element exists
    expect(comp.nativeElement.textContent).not.toContain('How can I help you today?');
  });

  it('should show streaming indicator when streaming', () => {
    mockStore.activeConversationId.set(1);
    mockStore.isStreaming.set(true);
    mockStore.streamingContent.set('typing...');
    const comp = createComponent();
    expect(comp.nativeElement.textContent).toContain('typing...');
  });

  it('should show error message with retry button', () => {
    mockStore.activeConversationId.set(1);
    mockStore.error.set('Something went wrong');
    const comp = createComponent();
    expect(comp.nativeElement.textContent).toContain('Something went wrong');
    expect(comp.nativeElement.textContent).toContain('Retry');
  });

  it('should call retryLastMessage on error retry click', () => {
    mockStore.activeConversationId.set(1);
    mockStore.error.set('Error!');
    const comp = createComponent();
    const retryBtn = Array.from(comp.nativeElement.querySelectorAll('button')).find(
      (b: Element) => b.textContent === 'Retry',
    );
    (retryBtn as HTMLElement)?.click();
    expect(mockStore.retryLastMessage).toHaveBeenCalled();
  });

  it('should show suggested prompts section when no messages', () => {
    mockStore.activeConversationId.set(1);
    mockStore.suggestedPrompts.set(['Ask plumbing', 'Ask electrical']);
    const comp = createComponent();
    const promptsEl = comp.nativeElement.querySelector('app-ai-suggested-prompts');
    expect(promptsEl).toBeTruthy();
  });

  it('should send message on input enter', () => {
    mockStore.activeConversationId.set(1);
    const comp = createComponent();
    const input = comp.nativeElement.querySelector('input')!;
    comp.componentInstance.inputText = 'Hello';
    const event = new KeyboardEvent('keydown', { key: 'Enter' });
    input.dispatchEvent(event);
    comp.detectChanges();
    expect(mockStore.sendMessage).toHaveBeenCalledWith('Hello');
  });

  it('should start new conversation if no active conv on send', () => {
    mockStore.activeConversationId.set(null);
    const comp = createComponent();
    comp.componentInstance.inputText = 'New chat';
    comp.componentInstance.sendMessage();
    expect(mockStore.startNewConversation).toHaveBeenCalledWith('New chat');
  });

  it('should not send if text is empty', () => {
    mockStore.activeConversationId.set(1);
    const comp = createComponent();
    comp.componentInstance.inputText = '   ';
    comp.componentInstance.sendMessage();
    expect(mockStore.sendMessage).not.toHaveBeenCalled();
  });

  it('should not send if streaming', () => {
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
    expect(comp.nativeElement.textContent).not.toContain('How can I help you today?');
  });

  it('should disable send button while streaming', () => {
    mockStore.activeConversationId.set(1);
    mockStore.isStreaming.set(true);
    const comp = createComponent();
    const sendBtn = Array.from(comp.nativeElement.querySelectorAll('button')).find(
      (b: Element) => b.innerHTML.includes('M12 19V5m0 0l-7 7m7-7l7 7'),
    );
    expect((sendBtn as HTMLButtonElement)?.disabled).toBe(true);
  });

  it('should handle suggestion click by sending message', () => {
    mockStore.activeConversationId.set(1);
    const comp = createComponent();
    comp.componentInstance.onSuggestionClick('Help me');
    expect(comp.componentInstance.inputText).toBe('');
    expect(mockStore.sendMessage).toHaveBeenCalledWith('Help me');
  });

  it('should call copyMessage with navigator clipboard', () => {
    const msg = makeMsg({ content: 'Copy this' });
    const writeText = vi.fn().mockResolvedValue(undefined);
    Object.assign(navigator, { clipboard: { writeText } });
    const comp = createComponent();
    comp.componentInstance.copyMessage(msg);
    expect(writeText).toHaveBeenCalledWith('Copy this');
  });
});
