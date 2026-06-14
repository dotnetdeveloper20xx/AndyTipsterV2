import { createReducer, on } from '@ngrx/store';
import { PermissionsActions } from './permissions.actions';
import { PermissionsState, initialPermissionsState } from './permissions.state';

export const permissionsReducer = createReducer(
  initialPermissionsState,

  on(PermissionsActions.loadPermissions, (state): PermissionsState => ({
    ...state,
    isLoading: true,
    error: null,
  })),

  on(PermissionsActions.loadPermissionsSuccess, (state, { permissions }): PermissionsState => ({
    ...state,
    allPermissions: permissions,
    isLoading: false,
    error: null,
  })),

  on(PermissionsActions.loadPermissionsFailure, (state, { error }): PermissionsState => ({
    ...state,
    isLoading: false,
    error,
  })),

  on(PermissionsActions.loadUserPermissionsSuccess, (state, { permissions }): PermissionsState => ({
    ...state,
    userPermissions: permissions,
  })),

  on(PermissionsActions.loadUserPermissionsFailure, (state, { error }): PermissionsState => ({
    ...state,
    error,
  })),

  on(PermissionsActions.clearPermissions, (): PermissionsState => ({
    ...initialPermissionsState,
  })),
);
