import { createReducer, on } from '@ngrx/store';
import { AuthActions } from './auth.actions';
import { AuthState, initialAuthState } from './auth.state';

export const authReducer = createReducer(
  initialAuthState,

  on(AuthActions.login, (state): AuthState => ({
    ...state,
    isLoading: true,
    error: null,
  })),

  on(AuthActions.loginSuccess, (state, { accessToken, refreshToken, expiresAt }): AuthState => ({
    ...state,
    accessToken,
    refreshToken,
    isAuthenticated: true,
    isLoading: false,
    error: null,
    requires2FA: false,
    tokenExpiresAt: expiresAt,
  })),

  on(AuthActions.loginFailure, (state, { error }): AuthState => ({
    ...state,
    isLoading: false,
    error,
  })),

  on(AuthActions.loginRequires2FA, (state): AuthState => ({
    ...state,
    isLoading: false,
    requires2FA: true,
  })),

  on(AuthActions.verify2FA, (state): AuthState => ({
    ...state,
    isLoading: true,
    error: null,
  })),

  on(AuthActions.verify2FASuccess, (state, { accessToken, refreshToken, expiresAt }): AuthState => ({
    ...state,
    accessToken,
    refreshToken,
    isAuthenticated: true,
    isLoading: false,
    error: null,
    requires2FA: false,
    tokenExpiresAt: expiresAt,
  })),

  on(AuthActions.verify2FAFailure, (state, { error }): AuthState => ({
    ...state,
    isLoading: false,
    error,
  })),

  on(AuthActions.register, (state): AuthState => ({
    ...state,
    isLoading: true,
    error: null,
  })),

  on(AuthActions.registerSuccess, (state): AuthState => ({
    ...state,
    isLoading: false,
    error: null,
  })),

  on(AuthActions.registerFailure, (state, { error }): AuthState => ({
    ...state,
    isLoading: false,
    error,
  })),

  on(AuthActions.logout, AuthActions.logoutSuccess, (): AuthState => ({
    ...initialAuthState,
  })),

  on(AuthActions.refreshTokenSuccess, (state, { accessToken, refreshToken, expiresAt }): AuthState => ({
    ...state,
    accessToken,
    refreshToken,
    tokenExpiresAt: expiresAt,
  })),

  on(AuthActions.refreshTokenFailure, (): AuthState => ({
    ...initialAuthState,
  })),

  on(AuthActions.clearError, (state): AuthState => ({
    ...state,
    error: null,
  })),
);
