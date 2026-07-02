import { Component, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AiSourceReference } from '../../models/ai.models';

@Component({
  selector: 'app-ai-source-references',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (sources().length > 0) {
      <div class="mt-2">
        <button
          (click)="expanded.set(!expanded())"
          class="text-xs text-slate-400 hover:text-primary transition-colors flex items-center gap-1 cursor-pointer"
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            class="w-3 h-3 transition-transform"
            [ngClass]="{ 'rotate-90': expanded() }"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            stroke-width="2"
          >
            <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7" />
          </svg>
          {{ sources().length }} source{{ sources().length > 1 ? 's' : '' }}
        </button>

        @if (expanded()) {
          <div class="mt-1 space-y-1">
            @for (source of sources(); track $index) {
              <div class="text-xs text-slate-400 bg-slate-50 dark:bg-slate-800 rounded-lg p-2">
                <span class="font-medium text-slate-500 dark:text-slate-300">
                  {{ source.sourceType | titlecase }}:
                </span>
                {{ source.title }}
                @if (source.relevanceScore > 0) {
                  <span class="text-slate-300">({{ (source.relevanceScore * 100).toFixed(0) }}%)</span>
                }
              </div>
            }
          </div>
        }
      </div>
    }
  `,
})
export class AiSourceReferencesComponent {
  readonly sources = input<AiSourceReference[]>([]);
  protected expanded = signal(false);
}
