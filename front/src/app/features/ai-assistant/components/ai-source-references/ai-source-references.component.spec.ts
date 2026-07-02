import { describe, it, expect, beforeEach } from 'vitest';
import { ɵSIGNAL } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AiSourceReferencesComponent } from './ai-source-references.component';
import { AiSourceReference } from '../../models/ai.models';

function makeSources(count: number): AiSourceReference[] {
  return Array.from({ length: count }, (_, i) => ({
    sourceType: 'Service',
    sourceId: i + 1,
    title: `Source ${i + 1}`,
    excerpt: 'Excerpt',
    relevanceScore: 0.95,
  }));
}

describe('AiSourceReferencesComponent', () => {
  beforeEach(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({ imports: [AiSourceReferencesComponent] });
  });


  it('should render nothing when sources is empty', () => {
    const fixture = TestBed.createComponent(AiSourceReferencesComponent);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent?.trim()).toBe('');
  });

  it('should show sources count button', () => {
    const fixture = TestBed.createComponent(AiSourceReferencesComponent);
    const node = fixture.componentInstance.sources[ɵSIGNAL];
    node.value = makeSources(3);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('3 sources');
  });

  it('should show singular "source" for one item', () => {
    const fixture = TestBed.createComponent(AiSourceReferencesComponent);
    const node = fixture.componentInstance.sources[ɵSIGNAL];
    node.value = makeSources(1);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('1 source');
  });

  it('should expand on click', () => {
    const fixture = TestBed.createComponent(AiSourceReferencesComponent);
    const node = fixture.componentInstance.sources[ɵSIGNAL];
    node.value = makeSources(2);
    fixture.detectChanges();
    const toggle = fixture.nativeElement.querySelector('button')!;
    toggle.click();
    fixture.detectChanges();
    const items = fixture.nativeElement.querySelectorAll('[class*="rounded-lg"]');
    expect(items.length).toBe(2);
  });

  it('should collapse on second click', () => {
    const fixture = TestBed.createComponent(AiSourceReferencesComponent);
    const node = fixture.componentInstance.sources[ɵSIGNAL];
    node.value = makeSources(1);
    fixture.detectChanges();
    const toggle = fixture.nativeElement.querySelector('button')!;
    toggle.click();
    fixture.detectChanges();
    toggle.click();
    fixture.detectChanges();
    const items = fixture.nativeElement.querySelectorAll('[class*="rounded-lg"]');
    expect(items.length).toBe(0);
  });

  it('should display source details when expanded', () => {
    const fixture = TestBed.createComponent(AiSourceReferencesComponent);
    const node = fixture.componentInstance.sources[ɵSIGNAL];
    node.value = makeSources(1);
    fixture.detectChanges();
    fixture.nativeElement.querySelector('button')!.click();
    fixture.detectChanges();
    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Service');
    expect(text).toContain('Source 1');
    expect(text).toContain('95%');
  });
});
