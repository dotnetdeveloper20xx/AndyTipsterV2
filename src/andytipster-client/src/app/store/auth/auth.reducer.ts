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

  on(AuthActions.loginSuccess, (state, { accessToken, refreshToken, expiresAt, user, roles, permissions }): AuthState => ({
    ...state,
    accessToken,
    refreshToken,
    user: user ?? state.user,
    roles: roles ?? state.roles,
    permissions: permissions ?? state.permissions,
    isAuthenticated: true,
    isLoading: false,
    error: null,
    requires2FA: false,
    twoFactorEmail: null,
    tokenExpiresAt: expiresAt,
  })),

  on(AuthActions.loginFailure, (state, { error }): AuthState => ({
    ...state,
    isLoading: false,
    error,
  })),

  on(AuthActions.loginRequires2FA, (state, { email }): AuthState => ({
    ...state,
    isLoading: false,
    requires2FA: true,
    twoFactorEmail: email,
  })),

  on(AuthActions.verify2FA, (state): AuthState => ({
    ...state,
    isLoading: true,
    error: null,
  })),

  on(AuthActions.verify2FASuccess, (state, { accessToken, refreshToken, expiresAt, user, roles, permissions }): AuthState => ({
    ...state,
    accessToken,
    refreshToken,
    user: user ?? state.user,
    roles: roles ?? state.roles,
    permissions: permissions ?? state.permissions,
    isAuthenticated: true,
    isLoading: false,
    error: null,
    requires2FA: false,
    twoFactorEmail: null,
    tokenExpiresAt: expiresAt,
  })),

  on(AuthActions.verify2FAFailure, (state, { error }): AuthState => ({
    ...state,
    isLoading: false,
    error,
  })),

  on(AuthActions.verifyRecoveryCode, (state): AuthState => ({
    ...state,
    isLoading: true,
    error: null,
  })),

  on(AuthActions.verifyRecoveryCodeSuccess, (state, { accessToken, refreshToken, expiresAt, user, roles, permissions }): AuthState => ({
    ...state,
    accessToken,
    refreshToken,
    user: user ?? state.user,
    roles: roles ?? state.roles,
    permissions: permissions ?? state.permissions,
    isAuthenticated: true,
    isLoading: false,
    error: null,
    requires2FA: false,
    twoFactorEmail: null,
    tokenExpiresAt: expiresAt,
  })),

  on(AuthActions.verifyRecoveryCodeFailure, (state, { error }): AuthState => ({
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

  on(AuthActions.forgotPassword, (state): AuthState => ({
    ...state,
    isLoading: true,
    error: null,
  })),

  on(AuthActions.forgotPasswordSuccess, (state): AuthState => ({
    ...state,
    isLoading: false,
    error: null,
  })),

  on(AuthActions.forgotPasswordFailure, (state, { error }): AuthState => ({
    ...state,
    isLoading: false,
    error,
  })),

  on(AuthActions.socialLogin, (state): AuthState => ({
    ...state,
    isLoading: true,
    error: null,
  })),

  on(AuthActions.logout, AuthActions.logoutSuccess, (): AuthState => ({
    ...initialAuthState,
  })),

  on(AuthActions.refreshTokenSuccess, (state, { accessToken, refreshToken, expiresAt, user, roles, permissions }): AuthState => ({
    ...state,
    accessToken,
    refreshToken,
    user: user ?? state.user,
    roles: roles ?? state.roles,
    permissions: permissions ?? state.permissions,
    isAuthenticated: true,
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
