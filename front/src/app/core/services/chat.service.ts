import { Injectable, inject } from '@angular/core';
import { ChatApiService } from '../../features/chat/services/chat.service';

@Injectable({ providedIn: 'root' })
export class ChatService {
  private chatApi = inject(ChatApiService);

  getConversations(page = 1, pageSize = 20) {
    return this.chatApi.getConversations(page, pageSize);
  }

  getMessages(conversationId: number, page = 1, pageSize = 50) {
    return this.chatApi.getMessages(conversationId, page, pageSize);
  }

  sendMessage(request: { conversationId: number; content: string; mediaUrl?: string }) {
    return this.chatApi.sendMessage(request);
  }

  searchConversations(query: string, page = 1, pageSize = 20) {
    return this.chatApi.searchConversations(query, page, pageSize);
  }

  searchMessages(query: string, page = 1, pageSize = 20) {
    return this.chatApi.searchMessages(query, page, pageSize);
  }

  getUnreadCount() {
    return this.chatApi.getUnreadCount();
  }
}
