import { Component, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ChatStore } from './signals/chat.store';
import { ChatApiService } from './services/chat.service';
import { ConversationListComponent } from './components/conversation-list';
import { ChatHeaderComponent } from './components/chat-header';
import { MessageFeedComponent } from './components/message-feed';
import { MessageInputComponent } from './components/message-input';
import { MessageResponse } from './models/chat.models';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [
    CommonModule,
    ConversationListComponent,
    ChatHeaderComponent,
    MessageFeedComponent,
    MessageInputComponent,
  ],
  template: `
    <div
      class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 bg-slate-50 dark:bg-slate-900 transition-colors duration-300 min-h-screen"
    >
      <div
        class="bg-white dark:bg-slate-950 rounded-2xl border border-slate-150 dark:border-slate-850 shadow-lg h-[75vh] flex overflow-hidden"
      >
        <!-- Conversation List -->
        <div
          class="w-full md:w-80 lg:w-96 border-l border-slate-150 dark:border-slate-850 flex flex-col h-full"
          [class.hidden]="store.activeConversation() && isMobileView()"
        >
          <app-conversation-list (conversationSelected)="onConversationSelected($event)" />
        </div>

        <!-- Message Area -->
        <div
          class="flex-grow flex flex-col h-full bg-slate-50/50 dark:bg-slate-900/20"
          [class.hidden]="!store.activeConversation() && isMobileView()"
        >
          @if (store.activeConversation(); as conv) {
            <app-chat-header (backClicked)="onBack()" />
            <app-message-feed
              (replyMsg)="onReply($event)"
              (editMsg)="onEdit($event)"
              (deleteMsg)="onDelete($event)"
            />
            <app-message-input (sendMessage)="onSend($event)" />
          } @else {
            <div
              class="flex-grow flex flex-col items-center justify-center text-center p-12 space-y-5"
            >
              <div
                class="w-20 h-20 rounded-full bg-slate-100 dark:bg-slate-900 flex items-center justify-center text-4xl shadow-sm"
              >
                &#x1F4AC;
              </div>
              <h3 class="text-base sm:text-lg font-black text-slate-700 dark:text-slate-250">
                اختر محادثة للبدء في التواصل
              </h3>
              <p class="text-xs sm:text-sm text-slate-400 max-w-xs leading-relaxed">
                اضغط على أي مستخدم في القائمة الجانبية لبدء المحادثة ومناقشة تفاصيل الحجز وأعمال
                الصيانة.
              </p>
            </div>
          }
        </div>
      </div>
    </div>
  `,
})
export default class ChatComponent implements OnInit, OnDestroy {
  store = inject(ChatStore);
  private api = inject(ChatApiService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  private editingMessage = signal<MessageResponse | null>(null);

  ngOnInit(): void {
    this.store.init();

    this.route.queryParams.subscribe(async (params) => {
      const withUserId = params['with'];
      const convId = params['conv'];

      if (convId) {
        try {
          const conv = await this.api.getConversation(Number(convId)).toPromise();
          if (conv) this.store.selectConversation(conv);
        } catch {}
      } else if (withUserId) {
        try {
          const conv = await this.api
            .createConversation({ participantUserId: Number(withUserId) })
            .toPromise();
          if (conv) {
            this.store.selectConversation(conv);
            this.router.navigate([], {
              queryParams: { conv: conv.id },
              queryParamsHandling: 'merge',
            });
          }
        } catch {}
      }
    });
  }

  ngOnDestroy(): void {
    this.store.destroy();
  }

  onConversationSelected(conv: any): void {
    // handled by store
  }

  onBack(): void {
    this.store.activeConversation.set(null);
  }

  onSend(text: string): void {
    if (this.editingMessage()) {
      const msg = this.editingMessage()!;
      this.store.editMessage(msg.id, text);
      this.editingMessage.set(null);
    } else {
      this.store.sendMessage(text);
    }
  }

  onReply(msg: MessageResponse): void {
    this.store.setReplyTo(msg);
  }

  onEdit(msg: MessageResponse): void {
    this.editingMessage.set(msg);
  }

  onDelete(messageId: number): void {
    if (confirm('هل أنت متأكد من حذف هذه الرسالة؟')) {
      this.store.deleteMessage(messageId);
    }
  }

  isMobileView(): boolean {
    if (typeof window !== 'undefined') {
      return window.innerWidth < 768;
    }
    return false;
  }
}
