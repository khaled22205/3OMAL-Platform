import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AiStore } from '../../services/ai-store.service';

@Component({
  selector: 'app-ai-conversation-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="flex flex-col h-full">
      <!-- Search -->
      <div class="p-3 border-b border-slate-100 dark:border-slate-800">
        <input
          [ngModel]="store.searchQuery()"
          (ngModelChange)="store.searchQuery.set($event)"
          type="text"
          placeholder="Search conversations..."
          class="w-full text-sm px-3 py-2 rounded-xl bg-slate-50 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-slate-700 dark:text-slate-300 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-primary/30 focus:border-primary transition-all"
        />
      </div>

      <!-- New conversation button -->
      <button
        (click)="store.startNewConversation()"
        class="mx-3 mt-2 mb-1 text-sm px-3 py-2 rounded-xl bg-primary/10 text-primary hover:bg-primary/20 transition-colors flex items-center justify-center gap-2 cursor-pointer font-medium"
      >
        <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M12 4v16m8-8H4" />
        </svg>
        New conversation
      </button>

      <!-- Conversation list -->
      <div class="flex-1 overflow-y-auto">
        @if (store.filteredConversations().length === 0) {
          <div class="text-center py-8 text-xs text-slate-400 px-4">
            {{ store.searchQuery() ? 'No conversations found' : 'No conversations yet' }}
          </div>
        }

        @for (conv of store.filteredConversations(); track conv.id) {
          <button
            (click)="store.selectConversation(conv.id)"
            class="w-full text-left px-4 py-3 hover:bg-slate-50 dark:hover:bg-slate-800/50 transition-colors border-b border-slate-50 dark:border-slate-800/50 cursor-pointer"
            [ngClass]="{ 'bg-primary/5 dark:bg-primary/10 border-r-2 border-r-primary': store.activeConversationId() === conv.id }"
          >
            <div class="text-sm font-medium text-slate-700 dark:text-slate-300 truncate">
              {{ conv.title }}
            </div>
            <div class="text-xs text-slate-400 mt-0.5">
              {{ conv.messageCount }} messages
              @if (conv.updatedAt) {
                <span class="ml-2">{{ conv.updatedAt | date: 'MMM d' }}</span>
              }
            </div>
          </button>
        }

        @if (store.hasMoreConversations()) {
          <button
            (click)="store.loadConversations(store.conversationsPage())"
            class="w-full text-center py-3 text-xs text-primary hover:text-primary-hover transition-colors cursor-pointer"
            [disabled]="store.conversationsLoading()"
          >
            {{ store.conversationsLoading() ? 'Loading...' : 'Load more' }}
          </button>
        }
      </div>
    </div>
  `,
})
export class AiConversationListComponent {
  store = inject(AiStore);
}
