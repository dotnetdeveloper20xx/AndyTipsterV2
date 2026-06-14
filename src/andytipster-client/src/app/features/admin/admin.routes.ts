import { Routes } from '@angular/router';
import { roleGuard } from '../../core/guards/role.guard';

export const ADMIN_ROUTES: Routes = [
  {
    path: '',
    canActivate: [roleGuard],
    data: { roles: ['Super Admin', 'Admin', 'Moderator'] },
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent),
      },
      {
        path: 'users',
        loadComponent: () => import('./pages/user-management/user-management.component').then(m => m.UserManagementComponent),
      },
      {
        path: 'plans',
        loadComponent: () => import('./pages/plan-management/plan-management.component').then(m => m.PlanManagementComponent),
      },
      {
        path: 'tips',
        loadComponent: () => import('./pages/tip-management/tip-management.component').then(m => m.TipManagementComponent),
      },
      {
        path: 'cms',
        loadComponent: () => import('./pages/cms/cms.component').then(m => m.CmsComponent),
      },
      {
        path: 'analytics',
        loadComponent: () => import('./pages/analytics/analytics.component').then(m => m.AnalyticsComponent),
      },
    ],
  },
];
