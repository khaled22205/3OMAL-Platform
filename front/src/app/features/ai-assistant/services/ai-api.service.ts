import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseApiService } from '../../../core/services/base-api.service';
import { PagedResult } from '../../../core/models/api.models';
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
  private readonly apiUrl = `${this.baseUrl}/ai`;

  getConversations(page = 1, pageSize = 20): Observable<PagedResult<AiConversationSummary>> {
    return this.get<PagedResult<AiConversationSummary>>(
      `/ai/conversations?page=${page}&pageSize=${pageSize}`,
    );
  }

  getConversation(id: number): Observable<AiConversationDetail> {
    return this.get<AiConversationDetail>(`/ai/conversations/${id}`);
  }

  startConversation(request: StartConversationRequest): Observable<AiConversationSummary> {
    return this.post<AiConversationSummary>(`/ai/conversations`, request);
  }

  deleteConversation(id: number): Observable<boolean> {
    return this.delete<boolean>(`/ai/conversations/${id}`);
  }

  searchConversations(
    query: string,
    page = 1,
    pageSize = 20,
  ): Observable<PagedResult<AiConversationSummary>> {
    return this.get<PagedResult<AiConversationSummary>>(
      `/ai/conversations/search?q=${encodeURIComponent(query)}&page=${page}&pageSize=${pageSize}`,
    );
  }

  sendMessage(conversationId: number, content: string): Observable<AiMessage> {
    return this.post<AiMessage>(`/ai/conversations/${conversationId}/messages`, {
      conversationId,
      content,
    } as SendAiMessageRequest);
  }

  getMessages(
    conversationId: number,
    page = 1,
    pageSize = 50,
  ): Observable<PagedResult<AiMessage>> {
    return this.get<PagedResult<AiMessage>>(
      `/ai/conversations/${conversationId}/messages?page=${page}&pageSize=${pageSize}`,
    );
  }

  getSuggestions(): Observable<AiSuggestedPrompts> {
    return this.get<AiSuggestedPrompts>(`/ai/suggestions`);
  }
}
