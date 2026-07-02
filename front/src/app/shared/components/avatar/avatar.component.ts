import { Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-avatar',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="relative inline-flex items-center justify-center rounded-full overflow-hidden flex-shrink-0 select-none"
      [ngClass]="sizeClass()"
    >
      @if (src(); as imgSrc) {
        <img
          [src]="imgSrc"
          [alt]="alt()"
          class="w-full h-full object-cover"
          (error)="onError()"
        />
      } @else {
        <span
          class="font-bold text-white uppercase"
          [ngClass]="textSizeClass()"
        >
          {{ initials() }}
        </span>
      }
    </div>
  `,
})
export class AvatarComponent {
  readonly src = input<string>();
  readonly alt = input('');
  readonly name = input<string>('');
  readonly size = input<'sm' | 'md' | 'lg' | 'xl'>('md');
  readonly color = input<string>('bg-primary');

  protected hasError = false;

  protected sizeClass = computed(() => {
    switch (this.size()) {
      case 'sm': return 'w-8 h-8';
      case 'md': return 'w-10 h-10';
      case 'lg': return 'w-14 h-14';
      case 'xl': return 'w-20 h-20';
    }
  });

  protected textSizeClass = computed(() => {
    switch (this.size()) {
      case 'sm': return 'text-xs';
      case 'md': return 'text-sm';
      case 'lg': return 'text-lg';
      case 'xl': return 'text-2xl';
    }
  });

  protected initials = computed(() => {
    const n = this.name();
    if (!n) return '?';
    const parts = n.trim().split(/\s+/);
    if (parts.length === 1) return parts[0].charAt(0).toUpperCase();
    return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
  });

  protected onError() {
    this.hasError = true;
  }
}
