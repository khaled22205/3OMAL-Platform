import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AiApiService } from './ai-api.service';
import { SessionService } from './session.service';
import { environment } from '../../../../environments/environment';

describe('AiApiService', () => {
  let service: AiApiService;
  let httpMock: HttpTestingController;
  let mockSessionService: { getSessionId: ReturnType<typeof vi.fn> };

  const fakeSessionId = 'sess-test-123';

  beforeEach(() => {
    mockSessionService = { getSessionId: vi.fn().mockReturnValue(fakeSessionId) };

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([])),
        provideHttpClientTesting(),
        AiApiService,
        { provide: SessionService, useValue: mockSessionService },
      ],
    });

    service = TestBed.inject(AiApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('getConversations should append sessionId query param', () => {
    service.getConversations(1, 20).subscribe();
    const req = httpMock.expectOne(
      `${environment.apiUrl}/ai/conversations?page=1&pageSize=20&sessionId=${fakeSessionId}`,
    );
    expect(req.request.method).toBe('GET');
    req.flush({ success: true, data: { items: [], page: 1, pageSize: 20, totalCount: 0 } });
  });

  it('getConversation should append sessionId', () => {
    service.getConversation(5).subscribe();
    const req = httpMock.expectOne(
      `${environment.apiUrl}/ai/conversations/5?sessionId=${fakeSessionId}`,
    );
    expect(req.request.method).toBe('GET');
    req.flush({ success: true, data: { id: 5 } });
  });

  it('startConversation should include sessionId in body', () => {
    service.startConversation({ title: 'Test' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/ai/conversations`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toMatchObject({
      title: 'Test',
      sessionId: fakeSessionId,
    });
    req.flush({ success: true, data: { id: 1 } });
  });

  it('deleteConversation should append sessionId', () => {
    service.deleteConversation(3).subscribe();
    const req = httpMock.expectOne(
      `${environment.apiUrl}/ai/conversations/3?sessionId=${fakeSessionId}`,
    );
    expect(req.request.method).toBe('DELETE');
    req.flush({ success: true, data: true });
  });

  it('searchConversations should append sessionId', () => {
    service.searchConversations('plumb', 1, 20).subscribe();
    const req = httpMock.expectOne(
      `${environment.apiUrl}/ai/conversations/search?q=plumb&page=1&pageSize=20&sessionId=${fakeSessionId}`,
    );
    expect(req.request.method).toBe('GET');
    req.flush({ success: true, data: { items: [] } });
  });

  it('sendMessage should include sessionId in body', () => {
    service.sendMessage(1, 'Hello').subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/ai/conversations/1/messages`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toMatchObject({
      conversationId: 1,
      content: 'Hello',
      sessionId: fakeSessionId,
    });
    req.flush({ success: true, data: { id: 1 } });
  });

  it('getMessages should append sessionId', () => {
    service.getMessages(1, 1, 50).subscribe();
    const req = httpMock.expectOne(
      `${environment.apiUrl}/ai/conversations/1/messages?page=1&pageSize=50&sessionId=${fakeSessionId}`,
    );
    expect(req.request.method).toBe('GET');
    req.flush({ success: true, data: { items: [] } });
  });

  it('getSuggestions should not include sessionId', () => {
    service.getSuggestions().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/ai/suggestions`);
    expect(req.request.method).toBe('GET');
    expect(req.request.url).not.toContain('sessionId');
    req.flush({ success: true, data: { prompts: [] } });
  });
});
