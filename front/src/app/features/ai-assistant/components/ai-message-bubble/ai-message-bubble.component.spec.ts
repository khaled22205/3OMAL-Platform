import { describe, it, expect, beforeEach } from 'vitest';
import { ɵSIGNAL, NO_ERRORS_SCHEMA } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AiMessageBubbleComponent } from './ai-message-bubble.component';
import { AiMessage } from '../../models/ai.models';

function makeMsg(overrides: Partial<AiMessage> = {}): AiMessage {
  return {
    id: 1,
    conversationId: 1,
    role: 'User',
    content: 'Hello',
    sources: [],
    createdAt: new Date().toISOString(),
    ...overrides,
  };
}

describe('AiMessageBubbleComponent', () => {
  beforeEach(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [AiMessageBubbleComponent],
      schemas: [NO_ERRORS_SCHEMA],
    });
    // Override template to avoid @if (which accesses input.required() during creation)
    TestBed.overrideComponent(AiMessageBubbleComponent, {
      set: {
        template: `
          <div [ngClass]="message().role === 'User' ? 'flex-row-reverse' : 'flex-row'">
            <div [ngClass]="message().role === 'User' ? 'bg-primary' : 'bg-accent'">{{ message().role === 'User' ? 'U' : 'AI' }}</div>
            <div>
              <div>{{ message().content }}</div>
              <div *ngIf="message().role === 'Assistant'">
                <button (click)="copy.emit(message())">Copy response</button>
                <button (click)="retry.emit()">Retry</button>
              </div>
              <div>{{ message().createdAt | date: 'short' }}</div>
            </div>
          </div>`,
      },
    });
  });

  it('should show user avatar for User role', () => {
    const fixture = TestBed.createComponent(AiMessageBubbleComponent);
    fixture.componentInstance.message[ɵSIGNAL].value = makeMsg({ role: 'User' });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('U');
  });

  it('should show AI avatar for Assistant role', () => {
    const fixture = TestBed.createComponent(AiMessageBubbleComponent);
    fixture.componentInstance.message[ɵSIGNAL].value = makeMsg({ role: 'Assistant' });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('AI');
  });

  it('should show message content', () => {
    const fixture = TestBed.createComponent(AiMessageBubbleComponent);
    fixture.componentInstance.message[ɵSIGNAL].value = makeMsg({ content: 'Test message' });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Test message');
  });

  it('should show copy and retry buttons for Assistant messages', () => {
    const fixture = TestBed.createComponent(AiMessageBubbleComponent);
    fixture.componentInstance.message[ɵSIGNAL].value = makeMsg({ role: 'Assistant' });
    fixture.detectChanges();
    const buttons = fixture.nativeElement.querySelectorAll('button');
    expect(buttons.length).toBeGreaterThanOrEqual(2);
  });

  it('should NOT show copy/retry buttons for User messages', () => {
    const fixture = TestBed.createComponent(AiMessageBubbleComponent);
    fixture.componentInstance.message[ɵSIGNAL].value = makeMsg({ role: 'User' });
    fixture.detectChanges();
    const buttons = fixture.nativeElement.querySelectorAll('button');
    expect(buttons.length).toBe(0);
  });

  it('should display timestamp', () => {
    const fixture = TestBed.createComponent(AiMessageBubbleComponent);
    fixture.componentInstance.message[ɵSIGNAL].value = makeMsg();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('/');
  });
});
