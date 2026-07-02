import { describe, it, expect, beforeEach, vi } from 'vitest';
import { of } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';

describe('AuthService', () => {
  let service: AuthService;
  let http: { post: ReturnType<typeof vi.fn>; get: ReturnType<typeof vi.fn> };
  let routerMock: { navigate: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    Object.defineProperty(globalThis, 'localStorage', {
      value: (() => {
        let store: Record<string, string> = {};
        return {
          getItem: (key: string) => store[key] ?? null,
          setItem: (key: string, value: string) => {
            store[key] = value;
          },
          removeItem: (key: string) => {
            delete store[key];
          },
          clear: () => {
            store = {};
          },
          get length() {
            return Object.keys(store).length;
          },
          key: (_: number) => null,
        };
      })(),
      writable: true,
      configurable: true,
    });

    http = { post: vi.fn(), get: vi.fn() };
    routerMock = { navigate: vi.fn() };
    service = new AuthService(http as any, routerMock as any);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('login', () => {
    it('should call POST /auth/login and set user signal', () => {
      const mockResponse = {
        success: true,
        data: {
          success: true,
          message: null,
          errors: null,
          accessToken: 'abc',
          refreshToken: 'def',
          expiresAt: new Date().toISOString(),
          user: {
            id: 1,
            firstName: 'John',
            lastName: 'Doe',
            email: 'john@test.com',
            phoneNumber: null,
            roles: ['Customer'],
          },
        },
        message: null,
      };
      http.post.mockReturnValue(of(mockResponse));

      let result: any;
      service.login({ email: 'john@test.com', password: '123456' }).subscribe((r) => (result = r));

      expect(http.post).toHaveBeenCalledWith(`${environment.apiUrl}/auth/login`, {
        email: 'john@test.com',
        password: '123456',
      });
    });
  });

  describe('register', () => {
    it('should call POST /auth/register', () => {
      const mockResponse = {
        success: true,
        data: {
          success: true,
          message: null,
          errors: null,
          accessToken: 'abc',
          refreshToken: 'def',
          expiresAt: new Date().toISOString(),
          user: {
            id: 1,
            firstName: 'John',
            lastName: 'Doe',
            email: 'john@test.com',
            phoneNumber: null,
            roles: ['Worker'],
          },
        },
        message: null,
      };
      http.post.mockReturnValue(of(mockResponse));

      let result: any;
      service
        .register({
          firstName: 'John',
          lastName: 'Doe',
          email: 'john@test.com',
          password: '123456',
          phoneNumber: '01000000000',
          userType: 'Worker',
        })
        .subscribe((r) => (result = r));

      expect(http.post).toHaveBeenCalledWith(`${environment.apiUrl}/auth/register`, {
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@test.com',
        password: '123456',
        phoneNumber: '01000000000',
        userType: 'Worker',
      });
    });
  });

  describe('logout', () => {
    it('should clear tokens and navigate to /login', () => {
      localStorage.setItem('access_token', 'abc');
      service.user.set({
        id: 1,
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@test.com',
        phoneNumber: null,
        roles: ['Customer'],
      });

      service.logout();

      expect(localStorage.getItem('access_token')).toBeNull();
      expect(localStorage.getItem('refresh_token')).toBeNull();
      expect(service.user()).toBeNull();
      expect(routerMock.navigate).toHaveBeenCalledWith(['/login']);
    });
  });

  describe('isAuthenticated', () => {
    it('should return true when user is set', () => {
      service.user.set({
        id: 1,
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@test.com',
        phoneNumber: null,
        roles: ['Customer'],
      });
      expect(service.isAuthenticated()).toBe(true);
    });

    it('should return false when user is null', () => {
      service.user.set(null);
      expect(service.isAuthenticated()).toBe(false);
    });
  });

  describe('refreshToken', () => {
    it('should call POST /auth/refresh', () => {
      localStorage.setItem('refresh_token', 'existing-refresh');
      const mockResponse = {
        success: true,
        data: {
          success: true,
          message: null,
          errors: null,
          accessToken: 'new-access',
          refreshToken: 'new-refresh',
          expiresAt: new Date().toISOString(),
          user: {
            id: 1,
            firstName: 'John',
            lastName: 'Doe',
            email: 'john@test.com',
            phoneNumber: null,
            roles: ['Customer'],
          },
        },
        message: null,
      };
      http.post.mockReturnValue(of(mockResponse));

      let result: any;
      service.refreshToken().subscribe((r) => (result = r));

      expect(http.post).toHaveBeenCalledWith(`${environment.apiUrl}/auth/refresh`, {
        refreshToken: 'existing-refresh',
      });
    });
  });

  describe('getAccessToken', () => {
    it('should return token from localStorage', () => {
      localStorage.setItem('access_token', 'my-token');
      expect(service.getAccessToken()).toBe('my-token');
    });

    it('should return null when no token', () => {
      localStorage.removeItem('access_token');
      expect(service.getAccessToken()).toBeNull();
    });
  });

  describe('restoreSession', () => {
    it('should call /auth/me when token exists', () => {
      localStorage.setItem('access_token', 'my-token');
      const mockResponse = {
        success: true,
        data: { userId: 1, email: 'john@test.com', roles: ['Customer'] },
        message: null,
      };
      http.get.mockReturnValue(of(mockResponse));

      service.restoreSession();

      expect(http.get).toHaveBeenCalledWith(`${environment.apiUrl}/auth/me`);
    });

    it('should do nothing when no token', () => {
      localStorage.removeItem('access_token');
      service.restoreSession();
      expect(http.get).not.toHaveBeenCalled();
    });
  });
});
