import { Injectable, inject } from '@angular/core';
import { SendMessageRequest } from '../../features/chat/models/chat.models';
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
    const payload: SendMessageRequest = {
      conversationId: request.conversationId,
      messageType: request.mediaUrl ? 'Image' : 'Text',
      content: request.content || null,
      replyToMessageId: null,
    };
    return this.chatApi.sendMessage(payload);
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
