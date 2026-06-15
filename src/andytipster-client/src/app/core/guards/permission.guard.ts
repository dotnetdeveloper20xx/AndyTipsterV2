import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateFn, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { map, take } from 'rxjs/operators';
import { combineLatest } from 'rxjs';
import { selectIsAuthenticated, selectAuthPermissions } from '../../store/auth/auth.selectors';

export const permissionGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const store = inject(Store);
  const router = inject(Router);
  const requiredPermissions: string[] = route.data['permissions'] ?? [];

  return combineLatest([
    store.select(selectIsAuthenticated),
    store.select(selectAuthPermissions),
  ]).pipe(
    take(1),
    map(([isAuthenticated, userPermissions]) => {
      if (!isAuthenticated) {
        return router.createUrlTree(['/auth/login']);
      }

      if (requiredPermissions.length === 0) {
        return true;
      }

      const hasRequiredPermission = requiredPermissions.some((perm) => userPermissions.includes(perm));
      if (hasRequiredPermission) {
        return true;
      }

      return router.createUrlTree(['/unauthorized']);
    })
  );
};
