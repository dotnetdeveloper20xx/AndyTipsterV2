import { createReducer, on } from '@ngrx/store';
import { UserActions } from './user.actions';
import { UserState, initialUserState } from './user.state';

export const userReducer = createReducer(
  initialUserState,

  on(UserActions.loadUser, (state): UserState => ({
    ...state,
    isLoading: true,
    error: null,
  })),

  on(UserActions.loadUserSuccess, (state, { profile }): UserState => ({
    ...state,
    profile,
    isLoading: false,
    error: null,
  })),

  on(UserActions.loadUserFailure, (state, { error }): UserState => ({
    ...state,
    isLoading: false,
    error,
  })),

  on(UserActions.updateProfile, (state): UserState => ({
    ...state,
    isLoading: true,
    error: null,
  })),

  on(UserActions.updateProfileSuccess, (state, { profile }): UserState => ({
    ...state,
    profile,
    isLoading: false,
  })),

  on(UserActions.updateProfileFailure, (state, { error }): UserState => ({
    ...state,
    isLoading: false,
    error,
  })),

  on(UserActions.uploadAvatarSuccess, (state, { avatarUrl }): UserState => ({
    ...state,
    profile: state.profile ? { ...state.profile, avatarUrl } : null,
  })),

  on(UserActions.uploadAvatarFailure, (state, { error }): UserState => ({
    ...state,
    error,
  })),

  on(UserActions.clearUser, (): UserState => ({
    ...initialUserState,
  })),

  on(UserActions.clearError, (state): UserState => ({
    ...state,
    error: null,
  })),
);
