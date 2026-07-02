import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { AiButtonComponent } from './ai-button.component';
import { AiStore } from '../../services/ai-store.service';

describe('AiButtonComponent', () => {
  let mockStore: { isOpen: ReturnType<typeof signal>; toggle: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    mockStore = {
      isOpen: signal(false),
      toggle: vi.fn(),
    };
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [AiButtonComponent],
      providers: [{ provide: AiStore, useValue: mockStore }],
    });
  });

  function createComponent() {
    const comp = TestBed.createComponent(AiButtonComponent);
    comp.detectChanges();
    return comp;
  }

  it('should render a button', () => {
    const comp = createComponent();
    const btn = comp.nativeElement.querySelector('button');
    expect(btn).toBeTruthy();
  });

  it('should show chat icon when closed', () => {
    mockStore.isOpen.set(false);
    const comp = createComponent();
    const svgs = comp.nativeElement.querySelectorAll('svg');
    expect(svgs.length).toBe(1);
  });

  it('should show close icon when open', () => {
    mockStore.isOpen.set(true);
    const comp = createComponent();
    const svgs = comp.nativeElement.querySelectorAll('svg');
    expect(svgs.length).toBe(1);
  });

  it('should call toggle on click', () => {
    const comp = createComponent();
    comp.nativeElement.querySelector('button')!.click();
    expect(mockStore.toggle).toHaveBeenCalledOnce();
  });

  it('should have primary background when closed', () => {
    const comp = createComponent();
    const btn = comp.nativeElement.querySelector('button');
    expect(btn?.className).not.toContain('bg-rose-500');
  });

  it('should have rose background when open', () => {
    mockStore.isOpen.set(true);
    const comp = createComponent();
    const btn = comp.nativeElement.querySelector('button');
    expect(btn?.className).toContain('bg-rose-500');
  });

  it('should show correct title when open', () => {
    mockStore.isOpen.set(true);
    const comp = createComponent();
    const btn = comp.nativeElement.querySelector('button');
    expect(btn?.getAttribute('title')).toBe('Close AI Assistant');
  });

  it('should show correct title when closed', () => {
    mockStore.isOpen.set(false);
    const comp = createComponent();
    const btn = comp.nativeElement.querySelector('button');
    expect(btn?.getAttribute('title')).toBe('Open AI Assistant');
  });
});
