import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { catchError, exhaustMap, map, tap } from 'rxjs/operators';
import { AuthService } from '../../core/services/auth.service';
import { AuthActions } from './auth.actions';

@Injectable()
export class AuthEffects {
  private readonly actions$ = inject(Actions);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  login$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.login),
      exhaustMap(({ email, password }) =>
        this.authService.login(email, password).pipe(
          map((response) => {
            if (response.requires2FA) {
              return AuthActions.loginRequires2FA();
            }
            return AuthActions.loginSuccess({
              accessToken: response.accessToken!,
              refreshToken: response.refreshToken!,
              expiresAt: response.expiresAt!,
            });
          }),
          catchError((error) =>
            of(AuthActions.loginFailure({ error: error.message ?? 'Login failed' }))
          )
        )
      )
    )
  );

  verify2FA$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.verify2FA),
      exhaustMap(({ code }) =>
        this.authService.verify2FA(code).pipe(
          map((response) =>
            AuthActions.verify2FASuccess({
              accessToken: response.accessToken,
              refreshToken: response.refreshToken,
              expiresAt: response.expiresAt,
            })
          ),
          catchError((error) =>
            of(AuthActions.verify2FAFailure({ error: error.message ?? '2FA verification failed' }))
          )
        )
      )
    )
  );

  register$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.register),
      exhaustMap(({ email, password, displayName }) =>
        this.authService.register(email, password, displayName).pipe(
          map(() => AuthActions.registerSuccess()),
          catchError((error) =>
            of(AuthActions.registerFailure({ error: error.message ?? 'Registration failed' }))
          )
        )
      )
    )
  );

  logout$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.logout),
      exhaustMap(() =>
        this.authService.logout().pipe(
          map(() => AuthActions.logoutSuccess()),
          catchError(() => of(AuthActions.logoutSuccess()))
        )
      )
    )
  );

  refreshToken$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.refreshToken),
      exhaustMap(() =>
        this.authService.refreshToken().pipe(
          map((response) =>
            AuthActions.refreshTokenSuccess({
              accessToken: response.accessToken,
              refreshToken: response.refreshToken,
              expiresAt: response.expiresAt,
            })
          ),
          catchError(() => of(AuthActions.refreshTokenFailure()))
        )
      )
    )
  );

  loginSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.loginSuccess, AuthActions.verify2FASuccess),
        tap(() => this.router.navigate(['/']))
      ),
    { dispatch: false }
  );

  logoutSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.logoutSuccess, AuthActions.refreshTokenFailure),
        tap(() => this.router.navigate(['/auth/login']))
      ),
    { dispatch: false }
  );
}
