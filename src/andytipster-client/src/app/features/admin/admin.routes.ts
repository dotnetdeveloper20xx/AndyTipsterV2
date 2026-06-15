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
        path: 'media-library',
        loadComponent: () => import('./pages/media-library/media-library.component').then(m => m.MediaLibraryComponent),
      },
      {
        path: 'navigation',
        loadComponent: () => import('./pages/navigation-editor/navigation-editor.component').then(m => m.NavigationEditorComponent),
      },
      {
        path: 'analytics',
        loadComponent: () => import('./pages/analytics/analytics.component').then(m => m.AnalyticsComponent),
      },
      {
        path: 'paypal-dashboard',
        loadComponent: () => import('./pages/paypal-dashboard/paypal-dashboard.component').then(m => m.PayPalDashboardComponent),
      },
      {
        path: 'audit',
        loadComponent: () => import('./pages/audit-log/audit-log.component').then(m => m.AuditLogComponent),
        data: { roles: ['Super Admin'] },
      },
      {
        path: 'blog',
        loadComponent: () => import('./pages/blog-management/blog-management.component').then(m => m.BlogManagementComponent),
      },
    ],
  },
];
