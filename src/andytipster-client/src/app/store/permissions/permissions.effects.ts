import { inject, Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { catchError, exhaustMap, map } from 'rxjs/operators';
import { PermissionsService } from '../../core/services/permissions.service';
import { AuthActions } from '../auth/auth.actions';
import { PermissionsActions } from './permissions.actions';

@Injectable()
export class PermissionsEffects {
  private readonly actions$ = inject(Actions);
  private readonly permissionsService = inject(PermissionsService);

  loadUserPermissions$ = createEffect(() =>
    this.actions$.pipe(
      ofType(PermissionsActions.loadUserPermissions, AuthActions.loginSuccess, AuthActions.verify2FASuccess),
      exhaustMap(() =>
        this.permissionsService.getUserPermissions().pipe(
          map((permissions) => PermissionsActions.loadUserPermissionsSuccess({ permissions })),
          catchError((error) =>
            of(PermissionsActions.loadUserPermissionsFailure({ error: error.message ?? 'Failed to load permissions' }))
          )
        )
      )
    )
  );

  loadAllPermissions$ = createEffect(() =>
    this.actions$.pipe(
      ofType(PermissionsActions.loadPermissions),
      exhaustMap(() =>
        this.permissionsService.getAllPermissions().pipe(
          map((permissions) => PermissionsActions.loadPermissionsSuccess({ permissions })),
          catchError((error) =>
            of(PermissionsActions.loadPermissionsFailure({ error: error.message ?? 'Failed to load permissions' }))
          )
        )
      )
    )
  );

  clearOnLogout$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.logoutSuccess, AuthActions.refreshTokenFailure),
      map(() => PermissionsActions.clearPermissions())
    )
  );
}
