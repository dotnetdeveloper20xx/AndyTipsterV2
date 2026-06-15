import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { map, take } from 'rxjs';
import { selectUserRoleNames } from '../../store/roles/roles.selectors';

export const subscriptionGuard: CanActivateFn = () => {
  const store = inject(Store);
  const router = inject(Router);

  return store.select(selectUserRoleNames).pipe(
    take(1),
    map((roles) => {
      // Subscribers, Admins, Super Admins, and Moderators can access subscriber content
      const hasAccess = roles.some(r =>
        ['Subscriber', 'Admin', 'Super Admin', 'Moderator'].includes(r)
      );
      if (!hasAccess) {
        router.navigate(['/pricing']);
        return false;
      }
      return true;
    })
  );
};
