import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot, provideRouter } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { authGuard, guestGuard } from './auth.guard';
import { signal, computed } from '@angular/core';

describe('authGuard', () => {
  let mockAuthService: {
    isAuthenticated: ReturnType<typeof computed<boolean>>;
    roles: ReturnType<typeof computed<string[]>>;
    navigateByRole: ReturnType<typeof vi.fn>;
  };
  let isAuthSignal: ReturnType<typeof signal<boolean>>;

  beforeEach(() => {
    isAuthSignal = signal(false);

    mockAuthService = {
      isAuthenticated: computed(() => isAuthSignal()),
      roles: computed(() => []),
      navigateByRole: vi.fn(),
    };

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: AuthService, useValue: mockAuthService }],
    });
  });

  it('returns true when authenticated', () => {
    isAuthSignal.set(true);

    const route = {} as ActivatedRouteSnapshot;
    const state = {} as RouterStateSnapshot;
    const result = TestBed.runInInjectionContext(() => authGuard(route, state));

    expect(result).toBe(true);
  });

  it('redirects to /login when not authenticated', () => {
    const route = {} as ActivatedRouteSnapshot;
    const state = {} as RouterStateSnapshot;
    const result = TestBed.runInInjectionContext(() => authGuard(route, state));

    const router = TestBed.inject(AuthService);
    expect(router.isAuthenticated()).toBe(false);
    expect(result).not.toBe(true);
  });
});

describe('guestGuard', () => {
  let mockAuthService: {
    isAuthenticated: ReturnType<typeof computed<boolean>>;
    roles: ReturnType<typeof computed<string[]>>;
    navigateByRole: ReturnType<typeof vi.fn>;
  };
  let isAuthSignal: ReturnType<typeof signal<boolean>>;

  beforeEach(() => {
    isAuthSignal = signal(false);

    mockAuthService = {
      isAuthenticated: computed(() => isAuthSignal()),
      roles: computed(() => []),
      navigateByRole: vi.fn(),
    };

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: AuthService, useValue: mockAuthService }],
    });
  });

  it('returns true when not authenticated', () => {
    const route = {} as ActivatedRouteSnapshot;
    const state = {} as RouterStateSnapshot;
    const result = TestBed.runInInjectionContext(() => guestGuard(route, state));

    expect(result).toBe(true);
  });

  it('redirects to / when authenticated', () => {
    isAuthSignal.set(true);

    const route = {} as ActivatedRouteSnapshot;
    const state = {} as RouterStateSnapshot;
    const result = TestBed.runInInjectionContext(() => guestGuard(route, state));

    expect(result).not.toBe(true);
  });
});
