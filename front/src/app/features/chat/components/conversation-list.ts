import { Component, inject, signal, computed, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatStore } from '../signals/chat.store';
import { ConversationResponse } from '../models/chat.models';
import { formatConversationTime, truncateText } from '../utils/chat-utils';

@Component({
  selector: 'app-conversation-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="flex flex-col h-full bg-white dark:bg-slate-950">
      <div class="p-4 border-b border-slate-100 dark:border-slate-850 space-y-3 text-right">
        <h2 class="text-lg font-black text-slate-800 dark:text-white">المحادثات</h2>
        <div class="relative">
          <input
            type="text"
            [(ngModel)]="searchQuery"
            (input)="onSearch()"
            placeholder="ابحث عن محادثة..."
            class="w-full pr-10 pl-4 py-2.5 bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-800 text-xs rounded-xl outline-none focus:border-primary text-right"
          />
          <span class="absolute right-3.5 top-1/2 -translate-y-1/2 text-slate-450">&#x1F50D;</span>
        </div>
      </div>

      <div class="flex-grow overflow-y-auto divide-y divide-slate-50 dark:divide-slate-900">
        @for (conv of filteredConvs(); track conv.id) {
          <div
            (click)="selectConv(conv)"
            [class]="
              store.activeConversation()?.id === conv.id
                ? 'bg-primary/5 border-r-4 border-primary'
                : 'hover:bg-slate-50 dark:hover:bg-slate-900/40'
            "
            class="p-4 flex items-center gap-3 cursor-pointer transition-colors text-right"
          >
            <div class="relative flex-shrink-0">
              <img
                [src]="
                  conv.otherUser.photo ||
                  'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=100'
                "
                class="w-11 h-11 rounded-xl object-cover"
              />
              @if (isOnline(conv.otherUser.userId)) {
                <span
                  class="absolute bottom-0 right-0 w-3 h-3 bg-accent border-2 border-white dark:border-slate-950 rounded-full"
                ></span>
              }
            </div>
            <div class="flex-grow space-y-1 overflow-hidden">
              <div class="flex items-center justify-between">
                <span class="text-xs text-slate-400 font-bold">
                  {{ conv.lastMessageAt ? formatTime(conv.lastMessageAt) : '' }}
                </span>
                <h3 class="text-xs sm:text-sm font-black text-slate-800 dark:text-white truncate">
                  {{ conv.otherUser.firstName }} {{ conv.otherUser.lastName }}
                </h3>
              </div>
              <div
                class="flex items-center justify-between text-xs text-slate-500 dark:text-slate-400"
              >
                @if (conv.unreadCount > 0) {
                  <span
                    class="px-2 py-0.5 bg-primary text-white font-extrabold text-[10px] rounded-full"
                  >
                    {{ conv.unreadCount }}
                  </span>
                } @else {
                  <span></span>
                }
                <p class="truncate max-w-[160px] font-semibold leading-normal">
                  {{
                    conv.lastMessage
                      ? truncate(conv.lastMessage.content || '&#x1F4CE; مرفق', 30)
                      : ''
                  }}
                </p>
              </div>
            </div>
          </div>
        } @empty {
          <div class="text-center py-12 text-xs text-slate-450 dark:text-slate-550">
            @if (searchQuery.trim()) {
              لا توجد نتائج للبحث
            } @else {
              لا توجد محادثات نشطة حالياً
            }
          </div>
        }

        @if (store.hasMoreConversations()) {
          <div #sentinel class="h-4"></div>
        }
      </div>
    </div>
  `,
})
export class ConversationListComponent {
  readonly store = inject(ChatStore);
  readonly conversationSelected = output<ConversationResponse>();

  searchQuery = '';
  isSearching = signal(false);

  filteredConvs = computed(() => {
    const all = this.store.conversations();
    const q = this.searchQuery.trim().toLowerCase();
    if (!q) return all;
    return all.filter((c) => {
      const name = `${c.otherUser.firstName} ${c.otherUser.lastName}`.toLowerCase();
      return name.includes(q);
    });
  });

  selectConv(conv: ConversationResponse): void {
    this.store.selectConversation(conv);
    this.conversationSelected.emit(conv);
  }

  onSearch(): void {
    this.isSearching.set(this.searchQuery.trim().length > 0);
  }

  isOnline(userId: number): boolean {
    return this.store.onlineUserIds().has(userId);
  }

  formatTime(dateStr: string): string {
    return formatConversationTime(dateStr);
  }

  truncate(text: string, max: number): string {
    return truncateText(text, max);
  }
}
