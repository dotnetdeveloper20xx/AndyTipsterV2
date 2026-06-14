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

export const selectTokenExpiresAt = createSelector(
  selectAuthState,
  (state) => state.tokenExpiresAt
);
