import { inject, Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { catchError, exhaustMap, map } from 'rxjs/operators';
import { UserService } from '../../core/services/user.service';
import { AuthActions } from '../auth/auth.actions';
import { UserActions } from './user.actions';

@Injectable()
export class UserEffects {
  private readonly actions$ = inject(Actions);
  private readonly userService = inject(UserService);

  loadUser$ = createEffect(() =>
    this.actions$.pipe(
      ofType(UserActions.loadUser, AuthActions.loginSuccess, AuthActions.verify2FASuccess),
      exhaustMap(() =>
        this.userService.getProfile().pipe(
          map((profile) => UserActions.loadUserSuccess({ profile })),
          catchError((error) =>
            of(UserActions.loadUserFailure({ error: error.message ?? 'Failed to load user' }))
          )
        )
      )
    )
  );

  updateProfile$ = createEffect(() =>
    this.actions$.pipe(
      ofType(UserActions.updateProfile),
      exhaustMap((action) =>
        this.userService.updateProfile(action).pipe(
          map((profile) => UserActions.updateProfileSuccess({ profile })),
          catchError((error) =>
            of(UserActions.updateProfileFailure({ error: error.message ?? 'Failed to update profile' }))
          )
        )
      )
    )
  );

  clearUserOnLogout$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.logoutSuccess, AuthActions.refreshTokenFailure),
      map(() => UserActions.clearUser())
    )
  );
}
