import { describe, it, expect, beforeEach } from 'vitest';
import { ɵSIGNAL } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AiSuggestedPromptsComponent } from './ai-suggested-prompts.component';

describe('AiSuggestedPromptsComponent', () => {
  beforeEach(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({ imports: [AiSuggestedPromptsComponent] });
  });


  it('should render nothing when prompts is empty (default)', () => {
    const fixture = TestBed.createComponent(AiSuggestedPromptsComponent);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent?.trim()).toBe('');
  });

  it('should render prompts as buttons', () => {
    const fixture = TestBed.createComponent(AiSuggestedPromptsComponent);
    const node = fixture.componentInstance.prompts[ɵSIGNAL];
    node.value = ['Ask about plumbing', 'Help with booking'];
    fixture.detectChanges();
    const buttons = fixture.nativeElement.querySelectorAll('button');
    expect(buttons.length).toBe(2);
    expect(buttons[0].textContent).toContain('Ask about plumbing');
    expect(buttons[1].textContent).toContain('Help with booking');
  });

  it('should emit select event on click', () => {
    const fixture = TestBed.createComponent(AiSuggestedPromptsComponent);
    const node = fixture.componentInstance.prompts[ɵSIGNAL];
    node.value = ['Hello'];
    fixture.detectChanges();
    const emitted: string[] = [];
    fixture.componentInstance.select.subscribe((p) => emitted.push(p));
    const btn = fixture.nativeElement.querySelector('button')!;
    btn.click();
    expect(emitted).toEqual(['Hello']);
  });
});
