import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-ai-suggested-prompts',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (prompts().length > 0) {
      <div class="flex flex-wrap gap-2 px-4 pb-3">
        @for (prompt of prompts(); track $index) {
          <button
            (click)="select.emit(prompt)"
            class="text-xs px-3 py-1.5 rounded-full border border-slate-200 dark:border-slate-700 text-slate-500 dark:text-slate-400 hover:border-primary hover:text-primary hover:bg-primary/5 transition-all duration-200 cursor-pointer whitespace-nowrap"
          >
            {{ prompt }}
          </button>
        }
      </div>
    }
  `,
})
export class AiSuggestedPromptsComponent {
  readonly prompts = input<string[]>([]);
  readonly select = output<string>();
}
