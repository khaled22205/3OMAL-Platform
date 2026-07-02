import { Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-skeleton',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="flex flex-col gap-3" role="status" aria-label="Loading">
      @switch (variant()) {
        @case ('text') {
          <div class="skeleton rounded-lg" [ngClass]="textWidthClass()" style="height: 14px;"></div>
          <div class="skeleton rounded-lg" style="width: 75%; height: 14px;"></div>
          <div class="skeleton rounded-lg" style="width: 60%; height: 14px;"></div>
        }
        @case ('card') {
          <div class="skeleton rounded-2xl" style="height: 200px;"></div>
          <div class="flex flex-col gap-2 p-2">
            <div class="skeleton rounded-lg" style="width: 70%; height: 16px;"></div>
            <div class="skeleton rounded-lg" style="width: 40%; height: 14px;"></div>
          </div>
        }
        @case ('circle') {
          <div class="skeleton rounded-full mx-auto" [ngClass]="circleSizeClass()"></div>
        }
        @case ('avatar-with-text') {
          <div class="flex items-center gap-3" [ngClass]="alignClass()">
            <div class="skeleton rounded-full w-12 h-12 flex-shrink-0"></div>
            <div class="flex flex-col gap-2 flex-grow">
              <div class="skeleton rounded-lg" style="width: 50%; height: 14px;"></div>
              <div class="skeleton rounded-lg" style="width: 30%; height: 12px;"></div>
            </div>
          </div>
        }
        @case ('table-row') {
          <div class="flex gap-4 p-3">
            <div class="skeleton rounded-lg flex-grow" style="height: 14px;"></div>
            <div class="skeleton rounded-lg flex-grow" style="height: 14px;"></div>
            <div class="skeleton rounded-lg" style="width: 80px; height: 14px;"></div>
          </div>
        }
        @default {
          <div class="skeleton rounded-lg" [ngClass]="customClass()" [style.height]="height() + 'px'"></div>
        }
      }
    </div>
  `,
})
export class SkeletonComponent {
  readonly variant = input<'text' | 'card' | 'circle' | 'avatar-with-text' | 'table-row' | 'custom'>('text');
  readonly width = input<string>();
  readonly height = input(16);
  readonly count = input(1);
  readonly align = input<'right' | 'left' | 'center'>('right');

  protected textWidthClass = computed(() => {
    const w = this.width();
    if (w) return w.startsWith('w-') ? w : `w-${w}`;
    return 'w-full';
  });

  protected circleSizeClass = computed(() => {
    const h = this.height();
    return `w-${h} h-${h}`;
  });

  protected alignClass = computed(() => {
    switch (this.align()) {
      case 'right': return 'flex-row';
      case 'left': return 'flex-row-reverse';
      case 'center': return 'flex-row justify-center';
    }
  });

  protected customClass = computed(() => {
    const w = this.width();
    return w ? (w.startsWith('w-') ? w : `w-${w}`) : 'w-full';
  });
}
