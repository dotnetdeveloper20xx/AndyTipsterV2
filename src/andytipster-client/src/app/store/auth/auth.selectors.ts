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

/**
 * Pre-defined cached role selectors for commonly checked roles.
 * Use these in components instead of calling selectHasRole() directly.
 * 
 * Note: selectHasRole and selectHasPermission factory functions above create new selector
 * instances on each call. Store the result as a class property in components rather than
 * calling them directly in templates to benefit from memoization.
 */
export const selectIsAdmin = createSelector(
  selectAuthRoles,
  (roles) => roles.includes('Admin') || roles.includes('Super Admin')
);

export const selectIsSuperAdmin = createSelector(
  selectAuthRoles,
  (roles) => roles.includes('Super Admin')
);

export const selectIsModerator = createSelector(
  selectAuthRoles,
  (roles) => roles.includes('Moderator')
);

export const selectIsSubscriber = createSelector(
  selectAuthRoles,
  (roles) => roles.includes('Subscriber')
);

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
