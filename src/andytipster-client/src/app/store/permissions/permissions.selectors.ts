import { createFeatureSelector, createSelector } from '@ngrx/store';
import { PermissionsState } from './permissions.state';

export const selectPermissionsState = createFeatureSelector<PermissionsState>('permissions');

export const selectAllPermissions = createSelector(
  selectPermissionsState,
  (state) => state.allPermissions
);

export const selectUserPermissions = createSelector(
  selectPermissionsState,
  (state) => state.userPermissions
);

export const selectHasPermission = (permission: string) =>
  createSelector(selectUserPermissions, (permissions) => permissions.includes(permission));

export const selectHasAnyPermission = (requiredPermissions: string[]) =>
  createSelector(selectUserPermissions, (permissions) =>
    requiredPermissions.some((p) => permissions.includes(p))
  );

export const selectHasAllPermissions = (requiredPermissions: string[]) =>
  createSelector(selectUserPermissions, (permissions) =>
    requiredPermissions.every((p) => permissions.includes(p))
  );

export const selectPermissionsIsLoading = createSelector(
  selectPermissionsState,
  (state) => state.isLoading
);

export const selectPermissionsError = createSelector(
  selectPermissionsState,
  (state) => state.error
);
