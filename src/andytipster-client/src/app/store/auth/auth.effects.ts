import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { catchError, exhaustMap, map, tap } from 'rxjs/operators';
import { AuthService } from '../../core/services/auth.service';
import { extractUserFromJwt } from '../../core/utils/jwt.utils';
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
            if (response.requiresTwoFactor) {
              return AuthActions.loginRequires2FA({ email: response.twoFactorEmail ?? email });
            }
            this.authService.storeTokens(response.accessToken!, response.refreshToken!);
            const extracted = extractUserFromJwt(response.accessToken!);
            return AuthActions.loginSuccess({
              accessToken: response.accessToken!,
              refreshToken: response.refreshToken!,
              expiresAt: response.expiresAt!,
              user: extracted?.user,
              roles: extracted?.roles,
              permissions: extracted?.permissions,
            });
          }),
          catchError((error) =>
            of(AuthActions.loginFailure({ error: error?.error?.message ?? error.message ?? 'Login failed' }))
          )
        )
      )
    )
  );

  verify2FA$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.verify2FA),
      exhaustMap(({ email, code }) =>
        this.authService.verify2FA(email, code).pipe(
          map((response) => {
            this.authService.storeTokens(response.accessToken, response.refreshToken);
            const extracted = extractUserFromJwt(response.accessToken);
            return AuthActions.verify2FASuccess({
              accessToken: response.accessToken,
              refreshToken: response.refreshToken,
              expiresAt: response.expiresAt,
              user: extracted?.user,
              roles: extracted?.roles,
              permissions: extracted?.permissions,
            });
          }),
          catchError((error) =>
            of(AuthActions.verify2FAFailure({ error: error?.error?.message ?? error.message ?? '2FA verification failed' }))
          )
        )
      )
    )
  );

  verifyRecoveryCode$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.verifyRecoveryCode),
      exhaustMap(({ email, code }) =>
        this.authService.verifyRecoveryCode(email, code).pipe(
          map((response) => {
            this.authService.storeTokens(response.accessToken, response.refreshToken);
            const extracted = extractUserFromJwt(response.accessToken);
            return AuthActions.verifyRecoveryCodeSuccess({
              accessToken: response.accessToken,
              refreshToken: response.refreshToken,
              expiresAt: response.expiresAt,
              user: extracted?.user,
              roles: extracted?.roles,
              permissions: extracted?.permissions,
            });
          }),
          catchError((error) =>
            of(AuthActions.verifyRecoveryCodeFailure({ error: error?.error?.message ?? error.message ?? 'Recovery code verification failed' }))
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
            of(AuthActions.registerFailure({ error: error?.error?.message ?? error.message ?? 'Registration failed' }))
          )
        )
      )
    )
  );

  forgotPassword$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.forgotPassword),
      exhaustMap(({ email }) =>
        this.authService.forgotPassword(email).pipe(
          map(() => AuthActions.forgotPasswordSuccess()),
          catchError((error) =>
            of(AuthActions.forgotPasswordFailure({ error: error?.error?.message ?? error.message ?? 'Request failed' }))
          )
        )
      )
    )
  );

  socialLogin$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.socialLogin),
      exhaustMap(({ provider, accessToken }) =>
        this.authService.socialLogin(provider, accessToken).pipe(
          map((response) => {
            if (response.requiresTwoFactor) {
              return AuthActions.loginRequires2FA({ email: response.twoFactorEmail ?? '' });
            }
            this.authService.storeTokens(response.accessToken!, response.refreshToken!);
            const extracted = extractUserFromJwt(response.accessToken!);
            return AuthActions.loginSuccess({
              accessToken: response.accessToken!,
              refreshToken: response.refreshToken!,
              expiresAt: response.expiresAt!,
              user: extracted?.user,
              roles: extracted?.roles,
              permissions: extracted?.permissions,
            });
          }),
          catchError((error) =>
            of(AuthActions.loginFailure({ error: error?.error?.message ?? error.message ?? 'Social login failed' }))
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
          map((response) => {
            this.authService.storeTokens(response.accessToken, response.refreshToken);
            const extracted = extractUserFromJwt(response.accessToken);
            return AuthActions.refreshTokenSuccess({
              accessToken: response.accessToken,
              refreshToken: response.refreshToken,
              expiresAt: response.expiresAt,
              user: extracted?.user,
              roles: extracted?.roles,
              permissions: extracted?.permissions,
            });
          }),
          catchError(() => of(AuthActions.refreshTokenFailure()))
        )
      )
    )
  );

  initAuth$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.initAuth),
      map(() => {
        const accessToken = this.authService.getStoredAccessToken();
        const refreshToken = this.authService.getStoredRefreshToken();
        if (accessToken && refreshToken) {
          const extracted = extractUserFromJwt(accessToken);
          if (extracted) {
            return AuthActions.loginSuccess({
              accessToken,
              refreshToken,
              expiresAt: '',
              user: extracted.user,
              roles: extracted.roles,
              permissions: extracted.permissions,
            });
          }
          return AuthActions.refreshToken();
        }
        return AuthActions.refreshTokenFailure();
      })
    )
  );

  loginSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.loginSuccess, AuthActions.verify2FASuccess, AuthActions.verifyRecoveryCodeSuccess),
        tap((action) => {
          // Don't navigate on initAuth restore (expiresAt is empty string when restoring from storage)
          if ('expiresAt' in action && action.expiresAt === '') return;
          this.router.navigate(['/']);
        })
      ),
    { dispatch: false }
  );

  loginRequires2FA$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.loginRequires2FA),
        tap(() => this.router.navigate(['/auth/2fa']))
      ),
    { dispatch: false }
  );

  logoutSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.logoutSuccess, AuthActions.refreshTokenFailure),
        tap(() => {
          this.authService.clearTokens();
          this.router.navigate(['/auth/login']);
        })
      ),
    { dispatch: false }
  );

  registerSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.registerSuccess),
        tap(() => this.router.navigate(['/auth/login']))
      ),
    { dispatch: false }
  );
}
