import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AiMessage } from '../../models/ai.models';
import { MarkdownRendererComponent } from '../../../../shared/components/markdown-renderer/markdown-renderer.component';
import { AiSourceReferencesComponent } from '../ai-source-references/ai-source-references.component';

@Component({
  selector: 'app-ai-message-bubble',
  standalone: true,
  imports: [CommonModule, MarkdownRendererComponent, AiSourceReferencesComponent],
  template: `
    <div
      class="flex gap-3 mb-4"
      [ngClass]="message().role === 'User' ? 'flex-row-reverse' : 'flex-row'"
    >
      <!-- Avatar -->
      <div
        class="w-8 h-8 rounded-full flex-shrink-0 flex items-center justify-center text-xs font-bold text-white shadow-sm"
        [ngClass]="message().role === 'User' ? 'bg-primary' : 'bg-accent'"
      >
        {{ message().role === 'User' ? 'U' : 'AI' }}
      </div>

      <!-- Message -->
      <div class="max-w-[85%] min-w-0">
        <div
          class="rounded-2xl px-4 py-3 shadow-sm"
          [ngClass]="message().role === 'User'
            ? 'bg-primary text-white rounded-tr-sm'
            : 'bg-slate-100 dark:bg-slate-800 text-slate-800 dark:text-slate-200 rounded-tl-sm'"
        >
          <app-markdown-renderer [content]="message().content" />

          @if (message().role === 'Assistant') {
            <app-ai-source-references [sources]="message().sources" />
          }
        </div>

        <!-- Timestamp -->
        <div
          class="text-[10px] text-slate-400 mt-1 px-1"
          [ngClass]="message().role === 'User' ? 'text-right' : 'text-left'"
        >
          {{ message().createdAt | date: 'short' }}
        </div>

        <!-- Actions -->
        @if (message().role === 'Assistant') {
          <div class="flex gap-2 mt-1 px-1" [ngClass]="message().role === 'User' ? 'justify-end' : 'justify-start'">
            <button
              (click)="copy.emit(message())"
              class="text-xs text-slate-400 hover:text-primary transition-colors cursor-pointer"
              title="Copy response"
            >
              <svg xmlns="http://www.w3.org/2000/svg" class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
              </svg>
            </button>
            <button
              (click)="retry.emit()"
              class="text-xs text-slate-400 hover:text-accent transition-colors cursor-pointer"
              title="Retry"
            >
              <svg xmlns="http://www.w3.org/2000/svg" class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
              </svg>
            </button>
          </div>
        }
      </div>
    </div>
  `,
})
export class AiMessageBubbleComponent {
  readonly message = input.required<AiMessage>();
  readonly copy = output<AiMessage>();
  readonly retry = output<void>();
}
