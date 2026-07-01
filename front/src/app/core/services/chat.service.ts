import { Injectable, inject } from '@angular/core';
import { MockDataService } from './mock-data.service';

@Injectable({ providedIn: 'root' })
export class ChatService {
  private mockData = inject(MockDataService);
  conversations = this.mockData.conversations;
  messages = this.mockData.messages;

  getMessagesBetween(userA: string, userB: string) {
    return this.messages().filter(m =>
      (m.senderId === userA && m.receiverId === userB) ||
      (m.senderId === userB && m.receiverId === userA)
    );
  }

  sendMessage(senderId: string, receiverId: string, content: string, image?: string) {
    this.mockData.sendMessage(senderId, receiverId, content, image);
  }
}
