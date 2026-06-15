import { createEntityAdapter, EntityAdapter } from '@ngrx/entity';
import { createReducer, on } from '@ngrx/store';
import { RolesActions } from './roles.actions';
import { Role, RolesState } from './roles.state';

export const rolesAdapter: EntityAdapter<Role> = createEntityAdapter<Role>({
  selectId: (role) => role.id,
  sortComparer: (a, b) => a.hierarchyLevel - b.hierarchyLevel,
});

export const initialRolesState: RolesState = rolesAdapter.getInitialState({
  userRoles: [],
  isLoading: false,
  error: null,
});

export const rolesReducer = createReducer(
  initialRolesState,

  on(RolesActions.loadRoles, (state): RolesState => ({
    ...state,
    isLoading: true,
    error: null,
  })),

  on(RolesActions.loadRolesSuccess, (state, { roles }): RolesState =>
    rolesAdapter.setAll(roles, { ...state, isLoading: false, error: null })
  ),

  on(RolesActions.loadRolesFailure, (state, { error }): RolesState => ({
    ...state,
    isLoading: false,
    error,
  })),

  on(RolesActions.loadUserRolesSuccess, (state, { roleIds }): RolesState => ({
    ...state,
    userRoles: roleIds,
  })),

  on(RolesActions.createRoleSuccess, (state, { role }): RolesState =>
    rolesAdapter.addOne(role, state)
  ),

  on(RolesActions.deleteRoleSuccess, (state, { roleId }): RolesState =>
    rolesAdapter.removeOne(roleId, state)
  ),

  on(RolesActions.createRoleFailure, RolesActions.deleteRoleFailure, (state, { error }): RolesState => ({
    ...state,
    error,
  })),

  on(RolesActions.clearRoles, (): RolesState => ({
    ...initialRolesState,
  })),
);
