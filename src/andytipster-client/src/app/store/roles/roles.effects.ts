import { inject, Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { catchError, exhaustMap, map } from 'rxjs/operators';
import { RolesService } from '../../core/services/roles.service';
import { AuthActions } from '../auth/auth.actions';
import { RolesActions } from './roles.actions';

@Injectable()
export class RolesEffects {
  private readonly actions$ = inject(Actions);
  private readonly rolesService = inject(RolesService);

  loadRoles$ = createEffect(() =>
    this.actions$.pipe(
      ofType(RolesActions.loadRoles),
      exhaustMap(() =>
        this.rolesService.getRoles().pipe(
          map((roles) => RolesActions.loadRolesSuccess({ roles })),
          catchError((error) =>
            of(RolesActions.loadRolesFailure({ error: error.message ?? 'Failed to load roles' }))
          )
        )
      )
    )
  );

  loadUserRoles$ = createEffect(() =>
    this.actions$.pipe(
      ofType(RolesActions.loadUserRoles, AuthActions.loginSuccess, AuthActions.verify2FASuccess),
      exhaustMap(() =>
        this.rolesService.getUserRoles().pipe(
          map((roleIds) => RolesActions.loadUserRolesSuccess({ roleIds })),
          catchError((error) =>
            of(RolesActions.loadUserRolesFailure({ error: error.message ?? 'Failed to load user roles' }))
          )
        )
      )
    )
  );

  clearOnLogout$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.logoutSuccess, AuthActions.refreshTokenFailure),
      map(() => RolesActions.clearRoles())
    )
  );
}
