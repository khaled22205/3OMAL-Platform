import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { ChatStore } from './chat.store';
import { ChatApiService } from '../services/chat.service';
import { SignalrService } from '../services/signalr.service';
import { AuthService } from '../../../core/services/auth.service';
import { signal, computed } from '@angular/core';
import { of } from 'rxjs';
import { ConversationResponse, MessageResponse } from '../models/chat.models';

describe('ChatStore', () => {
  let store: ChatStore;
  let mockApi: Record<string, ReturnType<typeof vi.fn>>;
  let mockSignalr: Record<string, ReturnType<typeof vi.fn> | Record<string, any>>;
  let mockAuth: { user: ReturnType<typeof signal>; getAccessToken: ReturnType<typeof vi.fn> };

  const mockConversations: ConversationResponse[] = [
    {
      id: 1,
      otherUser: { userId: 2, firstName: 'Ali', lastName: 'Ahmed', photo: null },
      lastMessage: null,
      unreadCount: 0,
      lastMessageAt: null,
    },
  ];

  const mockMessages: MessageResponse[] = [
    {
      id: 1, conversationId: 1, senderId: 2, senderName: 'Ali Ahmed',
      messageType: 'Text', content: 'Hello', replyToMessageId: null,
      replyToContent: null, attachments: [], createdAt: new Date().toISOString(),
      deliveredAt: null, readAt: null, editedAt: null, isEdited: false, isDeleted: false,
    },
  ];

  beforeEach(() => {
    mockApi = {
      getConversations: vi.fn().mockReturnValue(of({ items: mockConversations, page: 1, pageSize: 20, totalCount: 1, totalPages: 1, hasNextPage: false, hasPreviousPage: false })),
      getMessages: vi.fn().mockReturnValue(of({ items: mockMessages, page: 1, pageSize: 50, totalCount: 1, totalPages: 1, hasNextPage: false, hasPreviousPage: false })),
      getUnreadCount: vi.fn().mockReturnValue(of({ count: 0 })),
      searchConversations: vi.fn().mockReturnValue(of({ items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0, hasNextPage: false, hasPreviousPage: false })),
      searchMessages: vi.fn().mockReturnValue(of({ items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0, hasNextPage: false, hasPreviousPage: false })),
    };

    const isConnected = signal(false);
    mockSignalr = {
      startConnection: vi.fn().mockResolvedValue(undefined),
      stopConnection: vi.fn().mockResolvedValue(undefined),
      joinConversationGroup: vi.fn().mockResolvedValue(undefined),
      leaveConversationGroup: vi.fn().mockResolvedValue(undefined),
      sendMessage: vi.fn().mockResolvedValue(undefined),
      editMessage: vi.fn().mockResolvedValue(undefined),
      deleteMessage: vi.fn().mockResolvedValue(undefined),
      markAsRead: vi.fn().mockResolvedValue(undefined),
      startTyping: vi.fn().mockResolvedValue(undefined),
      stopTyping: vi.fn().mockResolvedValue(undefined),
      onNewMessage$: of(mockMessages[0]),
      onMessageEdited$: of(mockMessages[0]),
      onMessageDeleted$: of({ messageId: 1, userId: 2 }),
      onMessagesRead$: of({ conversationId: 1, readByUserId: 2, messageIds: [1] }),
      onUserTyping$: of({ conversationId: 1, userId: 2 }),
      onUserStoppedTyping$: of({ conversationId: 1, userId: 2 }),
      onUserOnline$: of(2),
      onUserOffline$: of(2),
    };

    mockAuth = {
      user: signal({ id: 1, firstName: 'Test', lastName: 'User', email: 'test@test.com', phoneNumber: null, roles: ['Customer'] }),
      getAccessToken: vi.fn().mockReturnValue('token'),
    };

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        ChatStore,
        { provide: ChatApiService, useValue: mockApi },
        { provide: SignalrService, useValue: mockSignalr },
        { provide: AuthService, useValue: mockAuth },
      ],
    });

    store = TestBed.inject(ChatStore);
  });

  it('should be created', () => {
    expect(store).toBeTruthy();
  });

  it('should initialize conversations on init', async () => {
    vi.spyOn(store, 'loadConversations').mockImplementation(() => Promise.resolve());
    
    store.init();
    
    expect(mockSignalr.startConnection).toHaveBeenCalled();
  });

  it('should load conversations', async () => {
    await store.loadConversations();

    expect(mockApi.getConversations).toHaveBeenCalledWith(1, 20);
    expect(store.conversations().length).toBe(1);
  });

  it('should load messages for active conversation', async () => {
    await store.loadConversations();
    store.selectConversation(mockConversations[0]);

    expect(mockApi.getMessages).toHaveBeenCalled();
    expect(store.activeConversation()).toBeTruthy();
  });

  it('should set replyToMessage', () => {
    store.setReplyTo(mockMessages[0]);
    expect(store.replyToMessage()?.id).toBe(1);
  });

  it('should clear replyToMessage when set to null', () => {
    store.setReplyTo(mockMessages[0]);
    store.setReplyTo(null);
    expect(store.replyToMessage()).toBeNull();
  });

  it('should start typing for active conversation', () => {
    store.selectConversation(mockConversations[0]);
    store.startTyping();
    expect(mockSignalr.startTyping).toHaveBeenCalledWith(1);
  });

  it('should not start typing without active conversation', () => {
    store.startTyping();
    expect(mockSignalr.startTyping).not.toHaveBeenCalled();
  });

  it('should stop typing for active conversation', () => {
    store.selectConversation(mockConversations[0]);
    store.stopTyping();
    expect(mockSignalr.stopTyping).toHaveBeenCalledWith(1);
  });

  it('should compute currentUserId from auth', () => {
    expect(store.currentUserId()).toBe(1);
  });

  it('should mark messages as read', async () => {
    store.selectConversation(mockConversations[0]);
    await new Promise(resolve => setTimeout(resolve, 0));
    store.markAsRead(mockConversations[0]);
    expect(mockSignalr.markAsRead).toHaveBeenCalled();
  });
});
