import { Component, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AiStore } from '../../services/ai-store.service';

@Component({
  selector: 'app-ai-button',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button
      (click)="store.toggle()"
      class="fixed bottom-6 left-6 z-50 w-14 h-14 rounded-full shadow-2xl flex items-center justify-center text-white transition-all duration-300 cursor-pointer hover:scale-110 active:scale-95"
      [ngClass]="store.isOpen() ? 'bg-rose-500 rotate-45' : 'bg-primary'"
      [title]="store.isOpen() ? 'Close AI Assistant' : 'Open AI Assistant'"
    >
      @if (store.isOpen()) {
        <svg xmlns="http://www.w3.org/2000/svg" class="w-7 h-7" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
          <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
        </svg>
      } @else {
        <svg xmlns="http://www.w3.org/2000/svg" class="w-7 h-7" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z" />
        </svg>
      }
    </button>
  `,
})
export class AiButtonComponent {
  store = inject(AiStore);
}
