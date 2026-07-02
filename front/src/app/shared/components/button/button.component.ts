import { Component, input, computed, output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-button',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button
      [type]="type()"
      [disabled]="disabled() || loading()"
      (click)="onClick.emit()"
      class="inline-flex items-center justify-center gap-2 font-bold rounded-xl transition-all duration-200 cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
      [ngClass]="[variantClass(), sizeClass()]"
    >
      @if (loading()) {
        <svg class="animate-spin" [ngClass]="loadingSizeClass()" fill="none" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
        </svg>
      }
      <ng-content />
    </button>
  `,
})
export class ButtonComponent {
  readonly variant = input<'primary' | 'secondary' | 'outline' | 'ghost' | 'danger'>('primary');
  readonly size = input<'sm' | 'md' | 'lg'>('md');
  readonly type = input<'button' | 'submit' | 'reset'>('button');
  readonly disabled = input(false);
  readonly loading = input(false);
  readonly onClick = output<void>();

  protected variantClass = computed(() => {
    switch (this.variant()) {
      case 'primary':
        return 'bg-primary hover:bg-primary-hover text-white shadow-md shadow-primary/20 hover:shadow-lg';
      case 'secondary':
        return 'bg-secondary hover:bg-secondary-hover text-white shadow-md';
      case 'outline':
        return 'bg-transparent border-2 border-primary text-primary hover:bg-primary hover:text-white';
      case 'ghost':
        return 'bg-transparent text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800';
      case 'danger':
        return 'bg-rose-600 hover:bg-rose-700 text-white shadow-md';
    }
  });

  protected sizeClass = computed(() => {
    switch (this.size()) {
      case 'sm': return 'px-4 py-2 text-sm';
      case 'md': return 'px-6 py-3 text-base';
      case 'lg': return 'px-8 py-4 text-lg';
    }
  });

  protected loadingSizeClass = computed(() => {
    switch (this.size()) {
      case 'sm': return 'w-4 h-4';
      case 'md': return 'w-5 h-5';
      case 'lg': return 'w-6 h-6';
    }
  });
}
