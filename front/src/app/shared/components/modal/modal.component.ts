import { Component, input, output, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (open()) {
      <div class="fixed inset-0 z-[999] flex items-center justify-center p-4">
        <!-- Backdrop -->
        <div
          class="absolute inset-0 bg-black/50 backdrop-blur-sm"
          (click)="onBackdropClick()"
        ></div>
        <!-- Content -->
        <div
          class="relative bg-white dark:bg-slate-950 rounded-2xl shadow-2xl border border-slate-100 dark:border-slate-800 w-full max-h-[90vh] overflow-y-auto animate-modal-in"
          [ngClass]="maxWidthClass"
        >
          @if (title() || closable()) {
            <div class="flex items-center justify-between p-6 border-b border-slate-100 dark:border-slate-800">
              @if (title()) {
                <h2 class="text-xl font-extrabold text-slate-850 dark:text-white text-right flex-grow">
                  {{ title() }}
                </h2>
              }
              @if (closable()) {
                <button
                  (click)="onClose.emit()"
                  class="p-2 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-xl transition-colors cursor-pointer flex-shrink-0"
                >
                  <svg class="w-5 h-5 text-slate-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              }
            </div>
          }
          <div class="p-6">
            <ng-content />
          </div>
        </div>
      </div>
    }
  `,
  styles: [
    `
      @keyframes modal-in {
        from { transform: scale(0.95) translateY(10px); opacity: 0; }
        to { transform: scale(1) translateY(0); opacity: 1; }
      }
      .animate-modal-in { animation: modal-in 0.2s cubic-bezier(0.16, 1, 0.3, 1) forwards; }
    `,
  ],
})
export class ModalComponent {
  readonly open = input(false);
  readonly title = input<string>();
  readonly closable = input(true);
  readonly closeOnBackdropClick = input(true);
  readonly maxWidth = input<'sm' | 'md' | 'lg' | 'xl' | 'full'>('md');
  readonly onClose = output<void>();

  @HostListener('document:keydown.escape')
  protected onEscape() {
    if (this.closable()) {
      this.onClose.emit();
    }
  }

  protected get maxWidthClass(): string {
    switch (this.maxWidth()) {
      case 'sm': return 'max-w-sm';
      case 'md': return 'max-w-md';
      case 'lg': return 'max-w-lg';
      case 'xl': return 'max-w-xl';
      case 'full': return 'max-w-full mx-4';
    }
  }

  protected onBackdropClick() {
    if (this.closeOnBackdropClick()) {
      this.onClose.emit();
    }
  }
}
