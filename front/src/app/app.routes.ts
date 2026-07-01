import { Routes } from '@angular/router';
import { authGuard, guestGuard, roleGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/pages/login/login'),
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/pages/register/register'),
  },
  {
    path: 'admin',
    canActivate: [authGuard, roleGuard(['Admin'])],
    loadComponent: () => import('./features/admin/dashboard/admin'),
  },
  {
    path: 'customer',
    canActivate: [authGuard, roleGuard(['Customer'])],
    loadComponent: () => import('./features/customer/dashboard/customer'),
  },
  {
    path: 'worker',
    canActivate: [authGuard, roleGuard(['Worker'])],
    loadComponent: () => import('./features/worker/dashboard/worker'),
  },
  {
    path: 'search',
    canActivate: [authGuard],
    loadComponent: () => import('./features/search/search'),
  },
  {
    path: 'profile/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./features/profile/profile'),
  },
  {
    path: 'booking/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./features/booking/booking'),
  },
  {
    path: 'chat',
    canActivate: [authGuard],
    loadComponent: () => import('./features/chat/chat'),
  },
  {
    path: 'customer-dashboard',
    canActivate: [authGuard, roleGuard(['Customer'])],
    loadComponent: () => import('./features/customer-dashboard/customer-dashboard'),
  },
  {
    path: 'worker-dashboard',
    canActivate: [authGuard, roleGuard(['Worker'])],
    loadComponent: () => import('./features/worker-dashboard/worker-dashboard'),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./features/home/home'),
  },
  {
    path: '**',
    redirectTo: 'login',
  },
];
