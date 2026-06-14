import { Routes } from '@angular/router';
import { unauthGuard } from '../../core/guards/unauth.guard';

export const AUTH_ROUTES: Routes = [
  {
    path: 'login',
    canActivate: [unauthGuard],
    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent),
  },
  {
    path: 'register',
    canActivate: [unauthGuard],
    loadComponent: () => import('./pages/register/register.component').then(m => m.RegisterComponent),
  },
  {
    path: 'forgot-password',
    canActivate: [unauthGuard],
    loadComponent: () => import('./pages/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent),
  },
  {
    path: '2fa',
    loadComponent: () => import('./pages/two-factor/two-factor.component').then(m => m.TwoFactorComponent),
  },
];
