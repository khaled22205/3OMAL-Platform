import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AiStore } from './services/ai-store.service';
import { AiButtonComponent } from './components/ai-button/ai-button.component';
import { AiChatWindowComponent } from './components/ai-chat-window/ai-chat-window.component';

@Component({
  selector: 'app-ai-assistant',
  standalone: true,
  imports: [CommonModule, AiButtonComponent, AiChatWindowComponent],
  template: `
    <app-ai-button />
    @if (store.isOpen()) {
      <app-ai-chat-window />
    }
  `,
})
export default class AiAssistantComponent implements OnInit, OnDestroy {
  store = inject(AiStore);

  ngOnInit(): void {
    this.store.init();
  }

  ngOnDestroy(): void {
    this.store.destroy();
  }
}
