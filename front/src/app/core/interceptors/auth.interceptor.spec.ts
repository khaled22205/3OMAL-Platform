import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let httpClient: HttpClient;
  let httpMock: HttpTestingController;
  let mockAuthService: {
    getAccessToken: ReturnType<typeof vi.fn>;
    refreshToken: ReturnType<typeof vi.fn>;
    logout: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    mockAuthService = {
      getAccessToken: vi.fn(),
      refreshToken: vi.fn(),
      logout: vi.fn(),
    };

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: mockAuthService },
      ],
    });

    httpClient = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('adds Bearer token to requests', () => {
    mockAuthService.getAccessToken.mockReturnValue('test-token');

    httpClient.get('/api/test').subscribe();

    const req = httpMock.expectOne('/api/test');
    expect(req.request.headers.get('Authorization')).toBe('Bearer test-token');
    req.flush({});
  });

  it('does not add token to auth requests', () => {
    mockAuthService.getAccessToken.mockReturnValue('test-token');

    httpClient.post('/auth/login', {}).subscribe();

    const req = httpMock.expectOne('/auth/login');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('does not add token to register requests', () => {
    mockAuthService.getAccessToken.mockReturnValue('test-token');

    httpClient.post('/auth/register', {}).subscribe();

    const req = httpMock.expectOne('/auth/register');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('handles 401 by refreshing token', () => {
    mockAuthService.getAccessToken.mockReturnValue('test-token');
    mockAuthService.refreshToken.mockReturnValue(
      of({
        success: true,
        message: null,
        errors: null,
        accessToken: 'new-token',
        refreshToken: 'new-refresh',
        expiresAt: new Date().toISOString(),
        user: { id: 1, firstName: '', lastName: '', email: '', phoneNumber: null, roles: [] },
      }),
    );

    httpClient.get('/api/test').subscribe();

    const req = httpMock.expectOne('/api/test');
    expect(req.request.headers.get('Authorization')).toBe('Bearer test-token');
    req.flush('', { status: 401, statusText: 'Unauthorized' });

    expect(mockAuthService.refreshToken).toHaveBeenCalled();

    const retryReq = httpMock.expectOne('/api/test');
    expect(retryReq.request.headers.get('Authorization')).toBe('Bearer new-token');
    retryReq.flush({});
  });

  it('handles 401 when refresh fails by logging out', () => {
    mockAuthService.getAccessToken.mockReturnValue('test-token');
    mockAuthService.refreshToken.mockReturnValue(throwError(() => new Error('Refresh failed')));

    httpClient.get('/api/test').subscribe({
      error: () => {},
    });

    const req = httpMock.expectOne('/api/test');
    req.flush('', { status: 401, statusText: 'Unauthorized' });

    expect(mockAuthService.logout).toHaveBeenCalled();
  });

  it('does not add token when no access token exists', () => {
    mockAuthService.getAccessToken.mockReturnValue(null);

    httpClient.get('/api/public').subscribe();

    const req = httpMock.expectOne('/api/public');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });
});
