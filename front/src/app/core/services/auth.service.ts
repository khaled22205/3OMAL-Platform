import { HttpClient } from '@angular/common/http';
import { Injectable, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { tap, Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  RefreshTokenRequest,
  UserInfo,
  ChangePasswordRequest,
} from '../models/auth.models';
import { User, WorkerProfile } from '../models/interfaces';
import { WrappedResponse } from '../models/api.models';

interface MeResponse {
  userId: number;
  email: string;
  roles: string[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private readonly accessTokenKey = 'access_token';
  private readonly refreshTokenKey = 'refresh_token';

  readonly user = signal<UserInfo | null>(null);
  readonly loading = signal(false);
  readonly isAuthenticated = computed(() => this.user() !== null);
  readonly roles = computed(() => this.user()?.roles ?? []);
  readonly isAdmin = computed(() => this.roles().includes('Admin'));
  readonly isWorker = computed(() => this.roles().includes('Worker'));
  readonly isCustomer = computed(() => this.roles().includes('Customer'));

  // UI adapter: wraps user() into A-compatible User format
  readonly currentUser = computed<User | null>(() => {
    const u = this.user();
    if (!u) return null;
    return {
      id: String(u.id),
      name: `${u.firstName} ${u.lastName}`.trim() || u.email,
      email: u.email,
      phone: u.phoneNumber || '',
      role: u.roles.includes('Worker')
        ? 'worker'
        : u.roles.includes('Customer')
          ? 'client'
          : 'client',
      avatar: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=100',
      createdAt: new Date().toISOString(),
    };
  });

  // Worker profile (mock for now)
  readonly currentWorkerProfile = signal<WorkerProfile | null>(null);

  constructor(
    private readonly http: HttpClient,
    private readonly router: Router,
  ) {}

  register(request: RegisterRequest): Observable<AuthResponse> {
    this.loading.set(true);
    return this.http.post<WrappedResponse<AuthResponse>>(`${this.apiUrl}/register`, request).pipe(
      map((w) => w.data),
      tap({
        next: (res) => {
          this.handleAuthResponse(res);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      }),
    );
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    this.loading.set(true);
    return this.http.post<WrappedResponse<AuthResponse>>(`${this.apiUrl}/login`, request).pipe(
      map((w) => w.data),
      tap({
        next: (res) => {
          this.handleAuthResponse(res);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      }),
    );
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return new Observable((observer) => {
        observer.error(new Error('No refresh token available'));
      });
    }
    const request: RefreshTokenRequest = { refreshToken };
    return this.http.post<WrappedResponse<AuthResponse>>(`${this.apiUrl}/refresh`, request).pipe(
      map((w) => w.data),
      tap({
        next: (res) => this.handleAuthResponse(res),
        error: () => this.logout(),
      }),
    );
  }

  logout(): void {
    const refreshToken = this.getRefreshToken();
    if (refreshToken) {
      this.http.post(`${this.apiUrl}/logout`, { refreshToken }).subscribe({ error: () => {} });
    }
    this.clearTokens();
    this.user.set(null);
    this.router.navigate(['/login']);
  }

  getCurrentUser(): Observable<AuthResponse> {
    return this.http.get<WrappedResponse<MeResponse>>(`${this.apiUrl}/me`).pipe(
      map((w) => ({
        success: true,
        message: null,
        errors: null,
        accessToken: null,
        refreshToken: null,
        expiresAt: new Date().toISOString(),
        user: {
          id: w.data.userId,
          firstName: '',
          lastName: '',
          email: w.data.email,
          phoneNumber: null,
          roles: w.data.roles,
        } satisfies UserInfo,
      })),
    );
  }

  changePassword(request: ChangePasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/change-password`, request);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.accessTokenKey);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.refreshTokenKey);
  }

  navigateByRole(): void {
    const roles = this.roles();
    if (roles.includes('Admin')) {
      this.router.navigate(['/admin']);
    } else if (roles.includes('Worker')) {
      this.router.navigate(['/worker-dashboard']);
    } else {
      this.router.navigate(['/customer-dashboard']);
    }
  }

  restoreSession(): void {
    const token = this.getAccessToken();
    if (!token) return;
    this.loading.set(true);
    this.getCurrentUser().subscribe({
      next: (res) => {
        if (res.success && res.user) {
          this.user.set(res.user);
        } else {
          this.refreshToken().subscribe({
            next: () => {},
            error: () => this.logout(),
          });
        }
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.logout();
      },
    });
  }

  private handleAuthResponse(res: AuthResponse): void {
    if (!res.success) return;
    if (res.accessToken) {
      localStorage.setItem(this.accessTokenKey, res.accessToken);
    }
    if (res.refreshToken) {
      localStorage.setItem(this.refreshTokenKey, res.refreshToken);
    }
    if (res.user) {
      this.user.set(res.user);
    }
  }

  private clearTokens(): void {
    localStorage.removeItem(this.accessTokenKey);
    localStorage.removeItem(this.refreshTokenKey);
  }
}
