import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateFn, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { map, take } from 'rxjs/operators';
import { selectIsAuthenticated, selectAuthRoles } from '../../store/auth/auth.selectors';
import { combineLatest } from 'rxjs';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const store = inject(Store);
  const router = inject(Router);
  const requiredRoles: string[] = route.data['roles'] ?? [];

  return combineLatest([
    store.select(selectIsAuthenticated),
    store.select(selectAuthRoles),
  ]).pipe(
    take(1),
    map(([isAuthenticated, userRoles]) => {
      if (!isAuthenticated) {
        return router.createUrlTree(['/auth/login']);
      }

      if (requiredRoles.length === 0) {
        return true;
      }

      const hasRequiredRole = requiredRoles.some((role) => userRoles.includes(role));
      if (hasRequiredRole) {
        return true;
      }

      return router.createUrlTree(['/unauthorized']);
    })
  );
};
