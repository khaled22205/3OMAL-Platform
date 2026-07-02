import { Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span
      class="inline-flex items-center gap-1 rounded-full font-bold whitespace-nowrap"
      [ngClass]="[variantClass(), sizeClass()]"
    >
      <ng-content />
    </span>
  `,
})
export class BadgeComponent {
  readonly variant = input<'default' | 'primary' | 'success' | 'warning' | 'danger' | 'info'>('default');
  readonly size = input<'sm' | 'md'>('sm');

  protected variantClass = computed(() => {
    switch (this.variant()) {
      case 'primary': return 'bg-primary/10 text-primary';
      case 'success': return 'bg-accent-light text-accent';
      case 'warning': return 'bg-amber-50 text-amber-600 dark:bg-amber-950 dark:text-amber-400';
      case 'danger': return 'bg-rose-50 text-rose-600 dark:bg-rose-950 dark:text-rose-400';
      case 'info': return 'bg-sky-50 text-sky-600 dark:bg-sky-950 dark:text-sky-400';
      default: return 'bg-slate-100 text-slate-600 dark:bg-slate-800 dark:text-slate-300';
    }
  });

  protected sizeClass = computed(() => {
    switch (this.size()) {
      case 'sm': return 'px-2 py-0.5 text-xs';
      case 'md': return 'px-2.5 py-1 text-sm';
    }
  });
}
