import { createFeatureSelector, createSelector } from '@ngrx/store';
import { UserState } from './user.state';

export const selectUserState = createFeatureSelector<UserState>('user');

export const selectUserProfile = createSelector(
  selectUserState,
  (state) => state.profile
);

export const selectUserIsLoading = createSelector(
  selectUserState,
  (state) => state.isLoading
);

export const selectUserError = createSelector(
  selectUserState,
  (state) => state.error
);

export const selectUserDisplayName = createSelector(
  selectUserProfile,
  (profile) => profile?.displayName ?? null
);

export const selectUserEmail = createSelector(
  selectUserProfile,
  (profile) => profile?.email ?? null
);
