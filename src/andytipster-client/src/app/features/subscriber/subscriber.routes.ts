import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';

export const SUBSCRIBER_ROUTES: Routes = [
  {
    path: '',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        redirectTo: 'tips',
        pathMatch: 'full',
      },
      {
        path: 'tips',
        loadComponent: () => import('./pages/tips-feed/tips-feed.component').then(m => m.TipsFeedComponent),
      },
      {
        path: 'results',
        loadComponent: () => import('./pages/results/results.component').then(m => m.ResultsComponent),
      },
      {
        path: 'billing',
        loadComponent: () => import('./pages/billing/billing.component').then(m => m.BillingComponent),
      },
      {
        path: 'checkout',
        loadComponent: () => import('./pages/checkout/checkout.component').then(m => m.CheckoutComponent),
      },
      {
        path: 'profile',
        loadComponent: () => import('./pages/profile/profile.component').then(m => m.ProfileComponent),
      },
    ],
  },
];
