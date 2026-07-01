import { Component, inject, ElementRef, ViewChild, AfterViewChecked, signal, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChatStore } from '../signals/chat.store';
import { MessageResponse } from '../models/chat.models';
import { formatChatTime } from '../utils/chat-utils';
import { TypingIndicatorComponent } from './typing-indicator';

@Component({
  selector: 'app-message-feed',
  standalone: true,
  imports: [CommonModule, TypingIndicatorComponent],
  template: `
    <div #scrollContainer class="flex-grow p-6 overflow-y-auto space-y-3">
      @if (store.hasMoreMessages()) {
        <div class="text-center py-4">
          @if (store.messagesLoading()) {
            <span class="text-xs text-slate-400 font-bold">جاري تحميل الرسائل...</span>
          } @else {
            <button (click)="loadOlder()" class="text-xs text-primary font-bold hover:underline cursor-pointer">
              تحميل المزيد
            </button>
          }
        </div>
      }

      @for (msg of store.messages(); track msg.id) {
        <div [class]="msg.senderId === store.currentUserId() ? 'justify-end' : 'justify-start'" class="flex w-full group">
          <div
            [class]="msg.senderId === store.currentUserId()
              ? 'bg-primary text-white rounded-2xl rounded-tr-none'
              : 'bg-white dark:bg-slate-950 border border-slate-100 dark:border-slate-850 text-slate-800 dark:text-slate-200 rounded-2xl rounded-tl-none'"
            class="p-3 max-w-sm sm:max-w-md shadow-sm relative space-y-1.5 text-right"
          >
            @if (msg.replyToContent) {
              <div class="text-[10px] opacity-70 border-r-2 pr-2 border-current mb-1">
                {{ msg.replyToContent | slice:0:50 }}
              </div>
            }

            @if (msg.isDeleted) {
              <p class="text-xs italic opacity-60">تم حذف الرسالة</p>
            } @else {
              @if (msg.messageType === 'Text' || msg.messageType === 'Emoji') {
                <p [class]="msg.messageType === 'Emoji' ? 'text-3xl' : 'text-xs sm:text-sm leading-relaxed font-semibold'">
                  {{ msg.content }}
                </p>
              }
              @if (msg.messageType === 'Image') {
                <div class="rounded-xl overflow-hidden max-h-48 mb-1">
                  <img [src]="msg.content" class="w-full h-full object-cover cursor-pointer" (click)="openImage(msg.content!)" />
                </div>
              }
              @if (msg.messageType === 'File') {
                <div class="flex items-center gap-2 p-2 rounded-lg bg-black/5 dark:bg-white/5">
                  <span class="text-lg">&#x1F4CE;</span>
                  <div class="flex-grow">
                    <p class="text-xs font-bold">{{ msg.attachments[0]?.fileName || 'ملف' }}</p>
                    <p class="text-[10px] opacity-70">{{ msg.attachments[0]?.fileSize ? formatSize(msg.attachments[0].fileSize) : '' }}</p>
                  </div>
                </div>
              }
              @if (msg.messageType === 'Hyperlink' && msg.content) {
                <a [href]="msg.content" target="_blank" class="text-xs underline break-all hover:opacity-80"
                  [class.text-white]="msg.senderId === store.currentUserId()"
                  [class.text-primary]="msg.senderId !== store.currentUserId()">
                  {{ msg.content }}
                </a>
              }
              @if (msg.messageType === 'Location' && msg.content) {
                <div class="text-xs">&#x1F4CD; {{ msg.content }}</div>
              }
            }

            <div [class]="msg.senderId === store.currentUserId() ? 'text-white/70' : 'text-slate-400'"
              class="text-[9px] block text-left flex items-center gap-1 justify-end">
              <span>{{ formatTime(msg.createdAt) }}</span>
              @if (msg.isEdited) {
                <span class="text-[8px]">(تم التعديل)</span>
              }
              @if (msg.senderId === store.currentUserId()) {
                @if (msg.readAt) {
                  <span class="text-[10px] text-blue-300">&#x2713;&#x2713;</span>
                } @else if (msg.deliveredAt) {
                  <span class="text-[10px]">&#x2713;&#x2713;</span>
                } @else {
                  <span class="text-[10px]">&#x2713;</span>
                }
              }
            </div>

            @if (msg.senderId === store.currentUserId()) {
              <div class="absolute top-1 left-1 hidden group-hover:flex gap-1">
                <button (click)="editMsg.emit(msg)" class="text-[10px] p-1 rounded hover:bg-black/10 cursor-pointer">&#x270F;</button>
                <button (click)="deleteMsg.emit(msg.id)" class="text-[10px] p-1 rounded hover:bg-black/10 cursor-pointer">&#x1F5D1;</button>
                <button (click)="replyMsg.emit(msg)" class="text-[10px] p-1 rounded hover:bg-black/10 cursor-pointer">&#x21A9;</button>
              </div>
            } @else {
              <div class="absolute top-1 left-1 hidden group-hover:flex gap-1">
                <button (click)="replyMsg.emit(msg)" class="text-[10px] p-1 rounded hover:bg-black/10 cursor-pointer">&#x21A9;</button>
              </div>
            }
          </div>
        </div>
      } @empty {
        <div class="text-center py-12 text-xs text-slate-400">
          لا توجد رسائل بعد. ابدأ المحادثة!
        </div>
      }

      <app-typing-indicator />
    </div>
  `,
  styles: [`
    :host { display: contents; }
  `]
})
export class MessageFeedComponent implements AfterViewChecked {
  store = inject(ChatStore);
  private el = inject(ElementRef);

  @ViewChild('scrollContainer') private scrollContainer!: ElementRef;

  readonly editMsg = output<MessageResponse>();
  readonly deleteMsg = output<number>();
  readonly replyMsg = output<MessageResponse>();

  private autoScroll = true;

  ngAfterViewChecked(): void {
    if (this.autoScroll) {
      this.scrollToBottom();
    }
  }

  private scrollToBottom(): void {
    try {
      const el = this.scrollContainer?.nativeElement;
      if (el) {
        el.scrollTop = el.scrollHeight;
      }
    } catch { }
  }

  loadOlder(): void {
    this.autoScroll = false;
    this.store.loadMessages();
  }

  formatTime(dateStr: string): string {
    return formatChatTime(dateStr);
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1048576) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / 1048576).toFixed(1)} MB`;
  }

  openImage(url: string): void {
    window.open(url, '_blank');
  }
}
