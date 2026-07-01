import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ChatStore } from '../../features/chat/signals/chat.store';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <nav class="sticky top-0 z-50 bg-white/80 dark:bg-slate-900/80 backdrop-blur-md border-b border-slate-100 dark:border-slate-800 transition-colors duration-300">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between h-16 sm:h-20">
          
          <!-- Logo & Brand -->
          <div class="flex items-center gap-8">
            <a routerLink="/" class="flex items-center gap-2 group">
              <div class="w-10 h-10 rounded-xl bg-primary flex items-center justify-center text-white font-extrabold text-xl shadow-lg shadow-primary/20 transform group-hover:scale-105 transition-transform duration-300">
                ع
              </div>
              <div class="flex flex-col">
                <span class="text-xl sm:text-2xl font-black text-slate-850 dark:text-white tracking-tight group-hover:text-primary transition-colors">عمال</span>
                <span class="text-[9px] font-bold text-slate-400 dark:text-slate-500 -mt-1 hidden sm:inline">أقرب صنايعي... في أسرع وقت.</span>
              </div>
            </a>

            <!-- Desktop Navigation Links -->
            <div class="hidden lg:flex items-center gap-6">
              <a routerLink="/" routerLinkActive="text-primary font-bold" [routerLinkActiveOptions]="{exact: true}" class="text-sm font-semibold text-slate-600 dark:text-slate-300 hover:text-primary dark:hover:text-primary transition-colors">الرئيسية</a>
              <a routerLink="/search" routerLinkActive="text-primary font-bold" class="text-sm font-semibold text-slate-600 dark:text-slate-300 hover:text-primary dark:hover:text-primary transition-colors">الخدمات والصنايعية</a>
              <a href="#how-it-works" class="text-sm font-semibold text-slate-600 dark:text-slate-300 hover:text-primary dark:hover:text-primary transition-colors">ازاي تشتغل؟</a>
              <a href="#faq" class="text-sm font-semibold text-slate-600 dark:text-slate-300 hover:text-primary dark:hover:text-primary transition-colors">الأسئلة الشائعة</a>
            </div>
          </div>

          <!-- Actions -->
          <div class="hidden md:flex items-center gap-4">
            
            <!-- Dark Mode Toggle -->
            <button 
              (click)="toggleDarkMode()" 
              class="p-2.5 rounded-xl hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-600 dark:text-slate-300 transition-colors cursor-pointer"
              title="تغيير المظهر"
            >
              @if (darkMode()) {
                <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364-6.364l-.707.707M6.343 17.657l-.707.707m12.728 0l-.707-.707M6.343 6.343l-.707-.707M12 8a4 4 0 100 8 4 4 0 000-8z" />
                </svg>
              } @else {
                <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
                </svg>
              }
            </button>

            <!-- Authentication State Buttons -->
            @if (authService.currentUser(); as user) {
              <!-- Chat icon -->
              <a 
                routerLink="/chat" 
                class="p-2.5 rounded-xl hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-600 dark:text-slate-300 transition-colors relative cursor-pointer"
                title="الرسائل"
              >
                <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
                </svg>
                @if (chatStore.unreadCount() > 0) {
                  <span class="absolute -top-1 -left-1 min-w-[18px] h-[18px] bg-red-500 text-white text-[10px] font-extrabold rounded-full flex items-center justify-center px-1">
                    {{ chatStore.unreadCount() > 99 ? '99+' : chatStore.unreadCount() }}
                  </span>
                }
              </a>

              <!-- User Profile Dropdown / Link -->
              <div class="flex items-center gap-3">
                <a 
                  [routerLink]="getDashboardRoute(user.role)" 
                  class="flex items-center gap-2.5 p-1.5 pe-4 rounded-xl bg-slate-50 hover:bg-slate-100 dark:bg-slate-800 dark:hover:bg-slate-700/80 border border-slate-100 dark:border-slate-700 transition-all cursor-pointer"
                >
                  <img [src]="user.avatar" class="w-8 h-8 rounded-lg object-cover" alt="Avatar">
                  <div class="flex flex-col text-right">
                    <span class="text-xs font-bold text-slate-700 dark:text-slate-200">{{ user.name }}</span>
                    <span class="text-[10px] text-slate-400 font-semibold leading-none">{{ getRoleLabel(user.role) }}</span>
                  </div>
                </a>

                <button 
                  (click)="authService.logout()" 
                  class="px-4 py-2 text-xs font-bold text-red-600 hover:text-white border border-red-200 dark:border-red-900 hover:bg-red-500 rounded-xl transition-all cursor-pointer"
                >
                  خروج
                </button>
              </div>
            } @else {
              <!-- Login & Register -->
              <a routerLink="/login" class="text-sm font-bold text-slate-700 dark:text-slate-200 hover:text-primary dark:hover:text-primary transition-colors cursor-pointer">تسجيل الدخول</a>
              <a routerLink="/register" class="px-5 py-2.5 text-sm font-bold bg-primary hover:bg-primary-hover text-white rounded-xl shadow-md shadow-primary/10 hover:shadow-lg transition-all cursor-pointer">انضم كصنايعي</a>
            }

          </div>

          <!-- Mobile menu button -->
          <div class="flex items-center gap-3 lg:hidden">
            <button 
              (click)="toggleDarkMode()" 
              class="p-2 rounded-xl hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-600 dark:text-slate-300 transition-colors"
            >
              @if (darkMode()) {
                <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364-6.364l-.707.707M6.343 17.657l-.707.707m12.728 0l-.707-.707M6.343 6.343l-.707-.707M12 8a4 4 0 100 8 4 4 0 000-8z" /></svg>
              } @else {
                <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" /></svg>
              }
            </button>

            <button 
              (click)="toggleMobileMenu()" 
              class="p-2 rounded-xl hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-600 dark:text-slate-300 transition-colors"
            >
              <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                @if (mobileMenuOpen()) {
                  <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
                } @else {
                  <path stroke-linecap="round" stroke-linejoin="round" d="M4 6h16M4 12h16M4 18h16" />
                }
              </svg>
            </button>
          </div>

        </div>
      </div>

      <!-- Mobile Navigation Drawer -->
      <div 
        *ngIf="mobileMenuOpen()" 
        class="lg:hidden bg-white dark:bg-slate-900 border-b border-slate-100 dark:border-slate-800 px-4 pt-2 pb-6 flex flex-col gap-4 animate-slide-up"
      >
        <a routerLink="/" (click)="mobileMenuOpen.set(false)" class="text-base font-bold text-slate-700 dark:text-slate-200 py-2 border-b border-slate-50 dark:border-slate-800/50">الرئيسية</a>
        <a routerLink="/search" (click)="mobileMenuOpen.set(false)" class="text-base font-bold text-slate-700 dark:text-slate-200 py-2 border-b border-slate-50 dark:border-slate-800/50">الخدمات والصنايعية</a>
        <a href="#how-it-works" (click)="mobileMenuOpen.set(false)" class="text-base font-bold text-slate-700 dark:text-slate-200 py-2 border-b border-slate-50 dark:border-slate-800/50">ازاي تشتغل؟</a>
        
        @if (authService.currentUser(); as user) {
          <a [routerLink]="getDashboardRoute(user.role)" (click)="mobileMenuOpen.set(false)" class="flex items-center gap-3 py-2 border-b border-slate-50 dark:border-slate-800/50">
            <img [src]="user.avatar" class="w-10 h-10 rounded-xl object-cover">
            <div class="flex flex-col text-right">
              <span class="text-sm font-bold text-slate-800 dark:text-slate-100">{{ user.name }}</span>
              <span class="text-xs text-slate-400 font-semibold">{{ getRoleLabel(user.role) }}</span>
            </div>
          </a>
          <a routerLink="/chat" (click)="mobileMenuOpen.set(false)" class="text-base font-bold text-slate-700 dark:text-slate-200 py-2 border-b border-slate-50 dark:border-slate-800/50">الرسائل</a>
          <button 
            (click)="authService.logout(); mobileMenuOpen.set(false)" 
            class="mt-2 w-full py-3 bg-red-50 text-red-650 dark:bg-red-950/20 dark:text-red-400 rounded-xl font-bold border border-red-100 dark:border-red-950/50"
          >
            تسجيل الخروج
          </button>
        } @else {
          <a routerLink="/login" (click)="mobileMenuOpen.set(false)" class="text-center text-base font-bold text-slate-700 dark:text-slate-200 py-3 rounded-xl border border-slate-200 dark:border-slate-800">تسجيل الدخول</a>
          <a routerLink="/register" (click)="mobileMenuOpen.set(false)" class="text-center text-base font-bold bg-primary hover:bg-primary-hover text-white py-3 rounded-xl shadow-md">انضم كصنايعي</a>
        }
      </div>
    </nav>
  `,
  styles: [`
    @keyframes slide-up {
      from { transform: translateY(-10px); opacity: 0; }
      to { transform: translateY(0); opacity: 1; }
    }
    .animate-slide-up {
      animation: slide-up 0.25s ease-out forwards;
    }
  `]
})
export class NavbarComponent {
  authService = inject(AuthService);
  chatStore = inject(ChatStore);
  darkMode = signal<boolean>(false);
  mobileMenuOpen = signal<boolean>(false);

  constructor() {
    if (typeof window !== 'undefined') {
      const savedDark = localStorage.getItem('darkMode') === 'true';
      this.darkMode.set(savedDark);
      this.applyTheme(savedDark);
    }
    if (this.authService.isAuthenticated()) {
      this.chatStore.init();
    }
  }

  toggleDarkMode() {
    const nextDark = !this.darkMode();
    this.darkMode.set(nextDark);
    localStorage.setItem('darkMode', String(nextDark));
    this.applyTheme(nextDark);
  }

  private applyTheme(dark: boolean) {
    if (typeof document !== 'undefined') {
      const html = document.documentElement;
      if (dark) {
        html.classList.add('dark');
      } else {
        html.classList.remove('dark');
      }
    }
  }

  toggleMobileMenu() {
    this.mobileMenuOpen.update(v => !v);
  }

  getRoleLabel(role: 'client' | 'worker'): string {
    switch (role) {
      case 'client': return 'حساب عميل';
      case 'worker': return 'حساب صنايعي';
    }
  }

  getDashboardRoute(role: 'client' | 'worker'): string {
    switch (role) {
      case 'client': return '/customer-dashboard';
      case 'worker': return '/worker-dashboard';
    }
  }
}
