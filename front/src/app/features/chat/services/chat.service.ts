import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  ConversationResponse,
  MessageResponse,
  SendMessageRequest,
  CreateConversationRequest,
  EditMessageRequest,
  MarkAsReadRequest,
  UnreadCountResponse,
  PagedResult,
  WrappedResponse,
} from '../models/chat.models';

@Injectable({ providedIn: 'root' })
export class ChatApiService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/chat`;

  getConversations(page = 1, pageSize = 20): Observable<PagedResult<ConversationResponse>> {
    return this.http.get<WrappedResponse<PagedResult<ConversationResponse>>>(`${this.apiUrl}/conversations?page=${page}&pageSize=${pageSize}`)
      .pipe(map(r => r.data));
  }

  getConversation(id: number): Observable<ConversationResponse> {
    return this.http.get<WrappedResponse<ConversationResponse>>(`${this.apiUrl}/conversations/${id}`)
      .pipe(map(r => r.data));
  }

  createConversation(request: CreateConversationRequest): Observable<ConversationResponse> {
    return this.http.post<WrappedResponse<ConversationResponse>>(`${this.apiUrl}/conversations`, request)
      .pipe(map(r => r.data));
  }

  getMessages(conversationId: number, page = 1, pageSize = 50): Observable<PagedResult<MessageResponse>> {
    return this.http.get<WrappedResponse<PagedResult<MessageResponse>>>(
      `${this.apiUrl}/conversations/${conversationId}/messages?page=${page}&pageSize=${pageSize}`)
      .pipe(map(r => r.data));
  }

  sendMessage(request: SendMessageRequest): Observable<MessageResponse> {
    return this.http.post<WrappedResponse<MessageResponse>>(`${this.apiUrl}/messages`, request)
      .pipe(map(r => r.data));
  }

  editMessage(id: number, request: EditMessageRequest): Observable<MessageResponse> {
    return this.http.put<WrappedResponse<MessageResponse>>(`${this.apiUrl}/messages/${id}`, request)
      .pipe(map(r => r.data));
  }

  deleteMessage(id: number): Observable<boolean> {
    return this.http.delete<WrappedResponse<{ deleted: boolean }>>(`${this.apiUrl}/messages/${id}`)
      .pipe(map(r => r.data.deleted));
  }

  markAsRead(request: MarkAsReadRequest): Observable<boolean> {
    return this.http.post<WrappedResponse<{ success: boolean }>>(`${this.apiUrl}/messages/read`, request)
      .pipe(map(r => r.data.success));
  }

  searchConversations(query: string, page = 1, pageSize = 20): Observable<PagedResult<ConversationResponse>> {
    return this.http.get<WrappedResponse<PagedResult<ConversationResponse>>>(
      `${this.apiUrl}/conversations/search?query=${encodeURIComponent(query)}&page=${page}&pageSize=${pageSize}`)
      .pipe(map(r => r.data));
  }

  searchMessages(query: string, page = 1, pageSize = 20): Observable<PagedResult<MessageResponse>> {
    return this.http.get<WrappedResponse<PagedResult<MessageResponse>>>(
      `${this.apiUrl}/messages/search?query=${encodeURIComponent(query)}&page=${page}&pageSize=${pageSize}`)
      .pipe(map(r => r.data));
  }

  getUnreadCount(): Observable<UnreadCountResponse> {
    return this.http.get<WrappedResponse<UnreadCountResponse>>(`${this.apiUrl}/unread-count`)
      .pipe(map(r => r.data));
  }
}
