import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ChatApiService } from './chat.service';
import { environment } from '../../../../environments/environment';

describe('ChatApiService', () => {
  let service: ChatApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([])),
        provideHttpClientTesting(),
        ChatApiService,
      ],
    });

    service = TestBed.inject(ChatApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getConversations should call correct URL', () => {
    service.getConversations(1, 20).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/chat/conversations?page=1&pageSize=20`);
    expect(req.request.method).toBe('GET');
    req.flush({ success: true, data: { items: [], page: 1, pageSize: 20, totalCount: 0 } });
  });

  it('createConversation should POST', () => {
    service.createConversation({ participantUserId: 2 }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/chat/conversations`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ participantUserId: 2 });
    req.flush({ success: true, data: { id: 1, otherUser: { userId: 2, firstName: 'A', lastName: 'B', photo: null }, lastMessage: null, unreadCount: 0, lastMessageAt: null } });
  });

  it('getMessages should call correct URL with params', () => {
    service.getMessages(1, 1, 50).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/chat/conversations/1/messages?page=1&pageSize=50`);
    expect(req.request.method).toBe('GET');
    req.flush({ success: true, data: { items: [], page: 1, pageSize: 50, totalCount: 0 } });
  });

  it('getUnreadCount should call correct URL', () => {
    service.getUnreadCount().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/chat/unread-count`);
    expect(req.request.method).toBe('GET');
    req.flush({ success: true, data: { count: 5 } });
  });

  it('sendMessage should POST to correct endpoint', () => {
    service.sendMessage({ conversationId: 1, messageType: 'Text', content: 'hello', replyToMessageId: null }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/chat/messages`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ conversationId: 1, messageType: 'Text', content: 'hello', replyToMessageId: null });
    req.flush({ success: true, data: { id: 1, conversationId: 1, senderId: 1, senderName: 'Test', messageType: 'Text', content: 'hello', replyToMessageId: null, replyToContent: null, attachments: [], createdAt: new Date().toISOString(), deliveredAt: null, readAt: null, editedAt: null, isEdited: false, isDeleted: false } });
  });

  it('deleteMessage should DELETE', () => {
    service.deleteMessage(1).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/chat/messages/1`);
    expect(req.request.method).toBe('DELETE');
    req.flush({ success: true, data: { deleted: true } });
  });
});
