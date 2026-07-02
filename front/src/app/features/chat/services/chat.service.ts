import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { BaseApiService } from '../../../core/services/base-api.service';
import {
  ConversationResponse,
  MessageResponse,
  SendMessageRequest,
  CreateConversationRequest,
  EditMessageRequest,
  MarkAsReadRequest,
  UnreadCountResponse,
  PagedResult,
} from '../models/chat.models';

@Injectable({ providedIn: 'root' })
export class ChatApiService extends BaseApiService {
  private readonly apiUrl = `${this.baseUrl}/chat`;

  getConversations(page = 1, pageSize = 20): Observable<PagedResult<ConversationResponse>> {
    return this.get<PagedResult<ConversationResponse>>(
      `/chat/conversations?page=${page}&pageSize=${pageSize}`,
    );
  }

  getConversation(id: number): Observable<ConversationResponse> {
    return this.get<ConversationResponse>(`/chat/conversations/${id}`);
  }

  createConversation(request: CreateConversationRequest): Observable<ConversationResponse> {
    return this.post<ConversationResponse>(`/chat/conversations`, request);
  }

  getMessages(
    conversationId: number,
    page = 1,
    pageSize = 50,
  ): Observable<PagedResult<MessageResponse>> {
    return this.get<PagedResult<MessageResponse>>(
      `/chat/conversations/${conversationId}/messages?page=${page}&pageSize=${pageSize}`,
    );
  }

  sendMessage(request: SendMessageRequest): Observable<MessageResponse> {
    return this.post<MessageResponse>(`/chat/messages`, request);
  }

  editMessage(id: number, request: EditMessageRequest): Observable<MessageResponse> {
    return this.put<MessageResponse>(`/chat/messages/${id}`, request);
  }

  deleteMessage(id: number): Observable<boolean> {
    return this.delete<{ deleted: boolean }>(`/chat/messages/${id}`).pipe(map((r) => r.deleted));
  }

  markAsRead(request: MarkAsReadRequest): Observable<boolean> {
    return this.post<{ success: boolean }>(`/chat/messages/read`, request).pipe(map((r) => r.success));
  }

  searchConversations(
    query: string,
    page = 1,
    pageSize = 20,
  ): Observable<PagedResult<ConversationResponse>> {
    return this.get<PagedResult<ConversationResponse>>(
      `/chat/conversations/search?query=${encodeURIComponent(query)}&page=${page}&pageSize=${pageSize}`,
    );
  }

  searchMessages(query: string, page = 1, pageSize = 20): Observable<PagedResult<MessageResponse>> {
    return this.get<PagedResult<MessageResponse>>(
      `/chat/messages/search?query=${encodeURIComponent(query)}&page=${page}&pageSize=${pageSize}`,
    );
  }

  getUnreadCount(): Observable<UnreadCountResponse> {
    return this.get<UnreadCountResponse>(`/chat/unread-count`);
  }
}
