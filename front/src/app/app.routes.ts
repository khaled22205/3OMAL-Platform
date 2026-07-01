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
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./features/home/home'),
  },
  {
    path: '**',
    redirectTo: 'login',
  },
];
