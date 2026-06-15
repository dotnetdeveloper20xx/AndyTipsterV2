import { createFeatureSelector, createSelector } from '@ngrx/store';
import { rolesAdapter } from './roles.reducer';
import { RolesState } from './roles.state';
import { selectAuthRoles } from '../auth/auth.selectors';

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

/**
 * Returns the current user's role names.
 * Reads from the AUTH state (populated from JWT on login) — the single source of truth.
 */
export const selectUserRoleNames = selectAuthRoles;

export const selectRolesIsLoading = createSelector(
  selectRolesState,
  (state) => state.isLoading
);

export const selectRolesError = createSelector(
  selectRolesState,
  (state) => state.error
);
