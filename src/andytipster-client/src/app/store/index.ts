import { ActionReducerMap } from '@ngrx/store';
import { AuthState, authReducer } from './auth';
import { UserState, userReducer } from './user';
import { RolesState } from './roles/roles.state';
import { rolesReducer } from './roles/roles.reducer';
import { PermissionsState, permissionsReducer } from './permissions';
import { TipsState } from './tips/tips.state';
import { tipsReducer } from './tips/tips.reducer';
import { PlansState } from './plans/plans.state';
import { plansReducer } from './plans/plans.reducer';

export interface AppState {
  auth: AuthState;
  user: UserState;
  roles: RolesState;
  permissions: PermissionsState;
  tips: TipsState;
  plans: PlansState;
}

export const reducers: ActionReducerMap<AppState> = {
  auth: authReducer,
  user: userReducer,
  roles: rolesReducer,
  permissions: permissionsReducer,
  tips: tipsReducer,
  plans: plansReducer,
};
