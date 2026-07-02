import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { ComponentFixture } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { NavbarComponent } from './navbar.component';
import { AuthService } from '../../core/services/auth.service';
import { ChatStore } from '../../features/chat/signals/chat.store';
import { signal, computed } from '@angular/core';

describe('NavbarComponent', () => {
  let fixture: ComponentFixture<NavbarComponent>;
  let component: NavbarComponent;
  let mockAuth: Record<string, any>;
  let mockChatStore: Record<string, any>;
  let currentUserSignal: ReturnType<typeof signal>;

  const mockUser = {
    id: '1',
    name: 'Test User',
    email: 'test@test.com',
    phone: '',
    role: 'client' as const,
    avatar: 'test.jpg',
    createdAt: new Date().toISOString(),
  };

  beforeEach(async () => {
    currentUserSignal = signal(null);

    mockAuth = {
      currentUser: computed(() => currentUserSignal()),
      isAuthenticated: computed(() => currentUserSignal() !== null),
      roles: computed(() => (currentUserSignal() ? ['Customer'] : [])),
      logout: vi.fn(),
      navigateByRole: vi.fn(),
    };

    mockChatStore = {
      unreadCount: signal(0),
      init: vi.fn(),
    };

    TestBed.resetTestingModule();
    await TestBed.configureTestingModule({
      imports: [NavbarComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: mockAuth },
        { provide: ChatStore, useValue: mockChatStore },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render login/register links when not authenticated', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const links = compiled.querySelectorAll('a');
    const loginLink = Array.from(links).find((l) => l.textContent?.includes('تسجيل الدخول'));
    expect(loginLink).toBeTruthy();
  });

  it('should show user name when authenticated', () => {
    currentUserSignal.set(mockUser);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Test User');
  });

  it('should toggle dark mode', () => {
    component.toggleDarkMode();
    expect(component.darkMode()).toBe(true);
    component.toggleDarkMode();
    expect(component.darkMode()).toBe(false);
  });

  it('should toggle mobile menu', () => {
    component.toggleMobileMenu();
    expect(component.mobileMenuOpen()).toBe(true);
    component.toggleMobileMenu();
    expect(component.mobileMenuOpen()).toBe(false);
  });

  it('getRoleLabel should return correct Arabic label', () => {
    expect(component.getRoleLabel('client')).toBe('حساب عميل');
    expect(component.getRoleLabel('worker')).toBe('حساب صنايعي');
  });

  it('getDashboardRoute should return correct route', () => {
    expect(component.getDashboardRoute('client')).toBe('/customer-dashboard');
    expect(component.getDashboardRoute('worker')).toBe('/worker-dashboard');
  });
});
