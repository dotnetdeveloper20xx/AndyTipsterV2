import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadChildren: () => import('./features/public/public.routes').then(m => m.PUBLIC_ROUTES),
  },
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES),
  },
  {
    path: 'subscriber',
    loadChildren: () => import('./features/subscriber/subscriber.routes').then(m => m.SUBSCRIBER_ROUTES),
  },
  {
    path: 'admin',
    loadChildren: () => import('./features/admin/admin.routes').then(m => m.ADMIN_ROUTES),
  },
  {
    path: 'unauthorized',
    loadComponent: () => import('./shared/pages/unauthorized/unauthorized.component').then(m => m.UnauthorizedComponent),
  },
  {
    path: '**',
    redirectTo: '',
  },
];
