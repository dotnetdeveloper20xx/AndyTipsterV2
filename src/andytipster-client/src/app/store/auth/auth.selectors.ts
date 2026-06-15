import { createFeatureSelector, createSelector } from '@ngrx/store';
import { AuthState } from './auth.state';

export const selectAuthState = createFeatureSelector<AuthState>('auth');

export const selectAccessToken = createSelector(
  selectAuthState,
  (state) => state.accessToken
);

export const selectRefreshToken = createSelector(
  selectAuthState,
  (state) => state.refreshToken
);

export const selectIsAuthenticated = createSelector(
  selectAuthState,
  (state) => state.isAuthenticated
);

export const selectAuthUser = createSelector(
  selectAuthState,
  (state) => state.user
);

export const selectAuthRoles = createSelector(
  selectAuthState,
  (state) => state.roles
);

export const selectAuthPermissions = createSelector(
  selectAuthState,
  (state) => state.permissions
);

export const selectHasRole = (role: string) =>
  createSelector(selectAuthRoles, (roles) => roles.includes(role));

export const selectHasPermission = (permission: string) =>
  createSelector(selectAuthPermissions, (permissions) => permissions.includes(permission));

export const selectAuthIsLoading = createSelector(
  selectAuthState,
  (state) => state.isLoading
);

export const selectAuthError = createSelector(
  selectAuthState,
  (state) => state.error
);

export const selectRequires2FA = createSelector(
  selectAuthState,
  (state) => state.requires2FA
);

export const selectTwoFactorEmail = createSelector(
  selectAuthState,
  (state) => state.twoFactorEmail
);

export const selectTokenExpiresAt = createSelector(
  selectAuthState,
  (state) => state.tokenExpiresAt
);
