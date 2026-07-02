import { describe, it, expect, beforeEach, vi } from 'vitest';
import { SessionService } from './session.service';

describe('SessionService', () => {
  let service: SessionService;

  beforeEach(() => {
    localStorage.clear();
    service = new SessionService();
  });

  it('getSessionId should create a new UUID when none exists', () => {
    const id = service.getSessionId();
    expect(id).toBeTruthy();
    expect(id).toMatch(/^[0-9a-f-]{36}$/);
  });

  it('getSessionId should return the same ID on repeated calls', () => {
    const first = service.getSessionId();
    const second = service.getSessionId();
    expect(second).toBe(first);
  });

  it('newSessionId should generate a different ID', () => {
    const first = service.getSessionId();
    const second = service.newSessionId();
    expect(second).not.toBe(first);
  });

  it('newSessionId should persist the new ID', () => {
    const id = service.newSessionId();
    const retrieved = service.getSessionId();
    expect(retrieved).toBe(id);
  });

  it('clearSessionId should remove stored ID', () => {
    service.getSessionId();
    service.clearSessionId();
    const after = service.getSessionId();
    expect(after).not.toBe('');
    // Should generate a fresh one
    expect(after).toMatch(/^[0-9a-f-]{36}$/);
  });
});
