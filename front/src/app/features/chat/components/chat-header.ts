import { Component, inject, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChatStore } from '../signals/chat.store';

@Component({
  selector: 'app-chat-header',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (store.activeConversation(); as conv) {
      <div
        class="px-6 py-4 bg-white dark:bg-slate-950 border-b border-slate-100 dark:border-slate-850 flex items-center justify-between"
      >
        <button
          (click)="backClicked.emit()"
          class="md:hidden p-2 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-900 text-slate-600 dark:text-slate-350 cursor-pointer"
        >
          &larr; رجوع
        </button>
        <div class="flex items-center gap-3 text-right">
          <div class="relative">
            <img
              [src]="
                conv.otherUser.photo ||
                'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=100'
              "
              class="w-10 h-10 rounded-xl object-cover"
            />
            @if (isOnline(conv.otherUser.userId)) {
              <span
                class="absolute bottom-0 right-0 w-2.5 h-2.5 bg-accent border-2 border-white dark:border-slate-950 rounded-full"
              ></span>
            }
          </div>
          <div class="flex flex-col">
            <h3 class="text-xs sm:text-sm font-black text-slate-800 dark:text-white">
              {{ conv.otherUser.firstName }} {{ conv.otherUser.lastName }}
            </h3>
            <span class="text-[10px] text-slate-400 font-bold">
              @if (isOnline(conv.otherUser.userId)) {
                متصل الآن
              } @else {
                غير متصل
              }
            </span>
          </div>
        </div>
        @if (isOnline(conv.otherUser.userId)) {
          <div class="flex items-center gap-2">
            <span class="w-2.5 h-2.5 rounded-full bg-accent animate-ping"></span>
            <span class="text-[10px] font-bold text-accent">متصل الآن</span>
          </div>
        }
      </div>
    }
  `,
})
export class ChatHeaderComponent {
  readonly store = inject(ChatStore);
  readonly backClicked = output<void>();

  isOnline(userId: number): boolean {
    return this.store.onlineUserIds().has(userId);
  }
}
