import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from '../../../core/services/base-api.service';
import { PagedResult } from '../../../core/models/api.models';
import { SessionService } from './session.service';
import {
  AiConversationSummary,
  AiConversationDetail,
  AiMessage,
  AiSuggestedPrompts,
  StartConversationRequest,
  SendAiMessageRequest,
} from '../models/ai.models';

@Injectable({ providedIn: 'root' })
export class AiApiService extends BaseApiService {
  private sessionService = inject(SessionService);
  private readonly apiUrl = `${this.baseUrl}/ai`;

  private sessionParam(): string {
    const sid = this.sessionService.getSessionId();
    return sid ? `&sessionId=${encodeURIComponent(sid)}` : '';
  }

  getConversations(page = 1, pageSize = 20): Observable<PagedResult<AiConversationSummary>> {
    return this.get<PagedResult<AiConversationSummary>>(
      `/ai/conversations?page=${page}&pageSize=${pageSize}${this.sessionParam()}`,
    );
  }

  getConversation(id: number): Observable<AiConversationDetail> {
    return this.get<AiConversationDetail>(
      `/ai/conversations/${id}?sessionId=${encodeURIComponent(this.sessionService.getSessionId())}`,
    );
  }

  startConversation(request: StartConversationRequest): Observable<AiConversationSummary> {
    const body = { ...request, sessionId: this.sessionService.getSessionId() };
    return this.post<AiConversationSummary>(`/ai/conversations`, body);
  }

  deleteConversation(id: number): Observable<boolean> {
    return this.delete<boolean>(
      `/ai/conversations/${id}?sessionId=${encodeURIComponent(this.sessionService.getSessionId())}`,
    );
  }

  searchConversations(
    query: string,
    page = 1,
    pageSize = 20,
  ): Observable<PagedResult<AiConversationSummary>> {
    return this.get<PagedResult<AiConversationSummary>>(
      `/ai/conversations/search?q=${encodeURIComponent(query)}&page=${page}&pageSize=${pageSize}${this.sessionParam()}`,
    );
  }

  sendMessage(conversationId: number, content: string): Observable<AiMessage> {
    return this.post<AiMessage>(`/ai/conversations/${conversationId}/messages`, {
      conversationId,
      content,
      sessionId: this.sessionService.getSessionId(),
    } as SendAiMessageRequest);
  }

  getMessages(
    conversationId: number,
    page = 1,
    pageSize = 50,
  ): Observable<PagedResult<AiMessage>> {
    return this.get<PagedResult<AiMessage>>(
      `/ai/conversations/${conversationId}/messages?page=${page}&pageSize=${pageSize}${this.sessionParam()}`,
    );
  }

  getSuggestions(): Observable<AiSuggestedPrompts> {
    return this.get<AiSuggestedPrompts>(`/ai/suggestions`);
  }
}
