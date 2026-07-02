import { Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="rounded-2xl border transition-all duration-200"
      [ngClass]="[paddingClass(), shadowClass(), hoverClass()]"
    >
      @if (title() || subtitle()) {
        <div class="flex flex-col gap-1 mb-4" [ngClass]="titleAlignClass()">
          @if (title()) {
            <h3 class="font-extrabold text-slate-850 dark:text-white text-lg">
              {{ title() }}
            </h3>
          }
          @if (subtitle()) {
            <p class="text-sm text-slate-500 dark:text-slate-400">{{ subtitle() }}</p>
          }
        </div>
      }
      <ng-content />
    </div>
  `,
})
export class CardComponent {
  readonly padding = input<'none' | 'sm' | 'md' | 'lg'>('md');
  readonly shadow = input<'none' | 'sm' | 'md' | 'lg'>('md');
  readonly hover = input(false);
  readonly title = input<string>();
  readonly subtitle = input<string>();
  readonly titleAlign = input<'center' | 'right' | 'left'>('right');

  protected paddingClass = computed(() => {
    switch (this.padding()) {
      case 'none': return '';
      case 'sm': return 'p-4';
      case 'md': return 'p-6';
      case 'lg': return 'p-8';
    }
  });

  protected shadowClass = computed(() => {
    switch (this.shadow()) {
      case 'none': return 'border-slate-100 dark:border-slate-800 bg-white dark:bg-slate-950';
      case 'sm': return 'border-slate-100 dark:border-slate-800 bg-white dark:bg-slate-950 shadow-sm';
      case 'md': return 'border-slate-100 dark:border-slate-800 bg-white dark:bg-slate-950 shadow-md';
      case 'lg': return 'border-slate-100 dark:border-slate-800 bg-white dark:bg-slate-950 shadow-lg';
    }
  });

  protected hoverClass = computed(() => {
    if (!this.hover()) return '';
    return 'hover:shadow-xl hover:border-primary dark:hover:border-primary hover:-translate-y-1 cursor-pointer';
  });

  protected titleAlignClass = computed(() => {
    switch (this.titleAlign()) {
      case 'center': return 'text-center';
      case 'right': return 'text-right';
      case 'left': return 'text-left';
    }
  });
}
