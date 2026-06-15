import { createFeatureSelector, createSelector } from '@ngrx/store';
import { rolesAdapter } from './roles.reducer';
import { RolesState } from './roles.state';

export const selectRolesState = createFeatureSelector<RolesState>('roles');

const { selectAll, selectEntities } = rolesAdapter.getSelectors();

export const selectAllRoles = createSelector(selectRolesState, selectAll);

export const selectRolesEntities = createSelector(selectRolesState, selectEntities);

export const selectUserRoleIds = createSelector(
  selectRolesState,
  (state) => state.userRoles
);

export const selectUserRoles = createSelector(
  selectAllRoles,
  selectUserRoleIds,
  (roles, userRoleIds) => roles.filter((r) => userRoleIds.includes(r.id))
);

export const selectUserRoleNames = createSelector(
  selectUserRoles,
  (roles) => roles.map((r) => r.name)
);

export const selectRolesIsLoading = createSelector(
  selectRolesState,
  (state) => state.isLoading
);

export const selectRolesError = createSelector(
  selectRolesState,
  (state) => state.error
);
