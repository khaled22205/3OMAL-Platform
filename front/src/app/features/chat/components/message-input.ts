import { Component, inject, signal, output, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatStore } from '../signals/chat.store';
import { MessageResponse } from '../models/chat.models';

@Component({
  selector: 'app-message-input',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="p-4 bg-white dark:bg-slate-950 border-t border-slate-100 dark:border-slate-850">
      @if (store.replyToMessage(); as reply) {
        <div
          class="flex items-center justify-between px-3 py-2 mb-2 bg-slate-50 dark:bg-slate-900 rounded-lg text-right"
        >
          <div class="flex-grow">
            <span class="text-[10px] font-bold text-primary block"
              >الرد على {{ reply.senderName }}</span
            >
            <span class="text-[10px] text-slate-500">{{
              reply.content || 'مرفق' | slice: 0 : 40
            }}</span>
          </div>
          <button
            (click)="store.setReplyTo(null)"
            class="text-red-500 text-xs font-bold cursor-pointer"
          >
            &times;
          </button>
        </div>
      }

      <div class="flex items-center gap-3">
        <button
          (click)="onSend()"
          [disabled]="!messageText().trim()"
          class="px-5 py-3 bg-primary hover:bg-primary-hover text-white font-bold rounded-xl shadow-md hover:shadow-lg transition-all text-xs sm:text-sm flex-shrink-0 cursor-pointer disabled:opacity-40 disabled:cursor-not-allowed"
        >
          إرسال
        </button>
        <input
          type="text"
          [ngModel]="messageText()"
          (ngModelChange)="onTextChange($event)"
          (keydown.enter)="onSend()"
          placeholder="اكتب رسالتك هنا..."
          class="flex-grow px-4 py-3 bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 text-xs sm:text-sm text-slate-800 dark:text-white rounded-xl outline-none focus:border-primary text-right font-medium"
        />
      </div>
    </div>
  `,
})
export class MessageInputComponent {
  readonly store = inject(ChatStore);
  readonly sendMessage = output<string>();

  messageText = signal('');

  private typingTimeout: any = null;

  onTextChange(value: string): void {
    this.messageText.set(value);
    this.store.startTyping();
    if (this.typingTimeout) clearTimeout(this.typingTimeout);
    this.typingTimeout = setTimeout(() => {
      this.store.stopTyping();
    }, 2000);
  }

  onSend(): void {
    const text = this.messageText().trim();
    if (!text) return;

    this.sendMessage.emit(text);
    this.messageText.set('');
    this.store.stopTyping();
    if (this.typingTimeout) {
      clearTimeout(this.typingTimeout);
      this.typingTimeout = null;
    }
  }
}
