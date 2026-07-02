import { Component, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChatStore } from '../signals/chat.store';

@Component({
  selector: 'app-typing-indicator',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (store.activeConversation(); as conv) {
      @for (userId of store.typingUserIds(); track userId) {
        @if (userId !== store.currentUserId()) {
          <div class="flex justify-start w-full animate-pulse">
            <div
              class="bg-white dark:bg-slate-950 border border-slate-100 dark:border-slate-850 p-3 rounded-2xl rounded-tl-none flex items-center gap-1.5"
            >
              <span
                class="w-2 h-2 rounded-full bg-slate-350 dark:bg-slate-500 animate-bounce"
                style="animation-delay: 0ms"
              ></span>
              <span
                class="w-2 h-2 rounded-full bg-slate-350 dark:bg-slate-500 animate-bounce"
                style="animation-delay: 150ms"
              ></span>
              <span
                class="w-2 h-2 rounded-full bg-slate-350 dark:bg-slate-500 animate-bounce"
                style="animation-delay: 300ms"
              ></span>
            </div>
          </div>
        }
      }
    }
  `,
})
export class TypingIndicatorComponent {
  readonly store = inject(ChatStore);
}
