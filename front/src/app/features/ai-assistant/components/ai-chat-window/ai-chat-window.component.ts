import { Component, inject, signal, viewChild, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AiStore } from '../../services/ai-store.service';
import { AiConversationListComponent } from '../ai-conversation-list/ai-conversation-list.component';
import { AiMessageBubbleComponent } from '../ai-message-bubble/ai-message-bubble.component';
import { AiSuggestedPromptsComponent } from '../ai-suggested-prompts/ai-suggested-prompts.component';
import { AiMessage } from '../../models/ai.models';

@Component({
  selector: 'app-ai-chat-window',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    AiConversationListComponent,
    AiMessageBubbleComponent,
    AiSuggestedPromptsComponent,
  ],
  template: `
    <div class="fixed bottom-24 left-6 z-40 w-[400px] max-w-[calc(100vw-3rem)] h-[600px] max-h-[calc(100vh-10rem)] bg-white dark:bg-slate-950 rounded-2xl shadow-2xl border border-slate-200 dark:border-slate-800 flex flex-col overflow-hidden transition-all duration-300">
      <!-- Header -->
      <div class="flex items-center justify-between px-4 py-3 border-b border-slate-100 dark:border-slate-800 bg-white dark:bg-slate-950">
        <div class="flex items-center gap-2">
          <div class="w-8 h-8 rounded-full bg-accent flex items-center justify-center text-white text-xs font-bold">
            AI
          </div>
          <div>
            <div class="text-sm font-bold text-slate-800 dark:text-slate-200">3OMAL AI</div>
            <div class="text-[10px] text-slate-400">Platform Assistant</div>
          </div>
        </div>
        <div class="flex items-center gap-1">
          @if (store.activeConversationId()) {
            <button
              (click)="store.clearConversation()"
              class="p-2 rounded-xl hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors text-slate-400 hover:text-rose-500 cursor-pointer"
              title="New conversation"
            >
              <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M12 4v16m8-8H4" />
              </svg>
            </button>
          }
          <button
            (click)="store.close()"
            class="p-2 rounded-xl hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors text-slate-400 hover:text-slate-600 cursor-pointer"
            title="Close"
          >
            <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
      </div>

      <!-- Body -->
      <div class="flex flex-1 overflow-hidden">
        <!-- Sidebar (conversations) -->
        @if (!store.activeConversationId()) {
          <div class="w-full overflow-hidden">
            <app-ai-conversation-list />
          </div>
        }

        <!-- Chat -->
        @if (store.activeConversationId() || store.isStreaming()) {
          <div class="flex-1 flex flex-col overflow-hidden">
            <!-- Messages -->
            <div #messageContainer class="flex-1 overflow-y-auto p-4 space-y-1">
              @if (store.messages().length === 0 && !store.isStreaming()) {
                <div class="text-center py-6">
                  <div class="text-2xl mb-2">👋</div>
                  <p class="text-sm text-slate-500 dark:text-slate-400">How can I help you today?</p>
                </div>
              }

              @for (msg of store.messages(); track msg.id || $index) {
                <app-ai-message-bubble
                  [message]="msg"
                  (copy)="copyMessage($event)"
                  (retry)="store.retryLastMessage()"
                />
              }

              @if (store.isStreaming()) {
                <div class="flex gap-3 mb-4">
                  <div class="w-8 h-8 rounded-full bg-accent flex-shrink-0 flex items-center justify-center text-xs font-bold text-white shadow-sm">AI</div>
                  <div class="bg-slate-100 dark:bg-slate-800 rounded-2xl rounded-tl-sm px-4 py-3 shadow-sm max-w-[85%]">
                    <div class="text-slate-800 dark:text-slate-200 text-sm leading-relaxed whitespace-pre-wrap">
                      {{ store.streamingContent() }}<span class="inline-block w-2 h-4 bg-primary animate-pulse ml-0.5"></span>
                    </div>
                  </div>
                </div>
              }

              @if (store.error()) {
                <div class="flex gap-3 mb-4">
                  <div class="w-8 h-8 rounded-full bg-rose-500 flex-shrink-0 flex items-center justify-center text-xs font-bold text-white shadow-sm">!</div>
                  <div class="bg-rose-50 dark:bg-rose-900/20 rounded-2xl rounded-tl-sm px-4 py-3 shadow-sm">
                    <p class="text-sm text-rose-600 dark:text-rose-400">{{ store.error() }}</p>
                    <button
                      (click)="store.retryLastMessage()"
                      class="text-xs text-rose-500 hover:text-rose-700 mt-1 underline cursor-pointer"
                    >Retry</button>
                  </div>
                </div>
              }
            </div>

            <!-- Suggested prompts -->
            @if (store.messages().length === 0 && !store.isStreaming()) {
              <app-ai-suggested-prompts
                [prompts]="store.suggestedPrompts()"
                (select)="onSuggestionClick($event)"
              />
            }

            <!-- Input -->
            <div class="border-t border-slate-100 dark:border-slate-800 p-3">
              <div class="flex gap-2">
                <input
                  #inputEl
                  [(ngModel)]="inputText"
                  (keydown.enter)="sendMessage()"
                  (keydown.shift.enter)="$event.preventDefault()"
                  type="text"
                  placeholder="Ask me anything..."
                  class="flex-1 text-sm px-4 py-2.5 rounded-xl bg-slate-50 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 text-slate-700 dark:text-slate-300 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-primary/30 focus:border-primary transition-all"
                  [disabled]="store.isStreaming()"
                />
                <button
                  (click)="sendMessage()"
                  class="px-4 py-2.5 rounded-xl bg-primary text-white hover:bg-primary-hover transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center cursor-pointer"
                  [disabled]="store.isStreaming() || !inputText.trim()"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M12 19V5m0 0l-7 7m7-7l7 7" />
                  </svg>
                </button>
              </div>
            </div>
          </div>
        }
      </div>
    </div>
  `,
})
export class AiChatWindowComponent implements AfterViewInit {
  store = inject(AiStore);
  inputText = '';

  private messageContainer = viewChild<ElementRef>('messageContainer');

  ngAfterViewInit(): void {
    this.scrollToBottom();
  }

  sendMessage(): void {
    const text = this.inputText.trim();
    if (!text || this.store.isStreaming()) return;

    this.inputText = '';

    if (!this.store.activeConversationId()) {
      this.store.startNewConversation(text);
    } else {
      this.store.sendMessage(text);
    }

    setTimeout(() => this.scrollToBottom(), 50);
  }

  onSuggestionClick(prompt: string): void {
    this.inputText = prompt;
    this.sendMessage();
  }

  copyMessage(msg: AiMessage): void {
    navigator.clipboard.writeText(msg.content);
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      const el = this.messageContainer()?.nativeElement;
      if (el) el.scrollTop = el.scrollHeight;
    }, 100);
  }
}
