/**
 * NgRx Store Architecture Notes:
 * 
 * The store is used for SHARED application state that multiple components need:
 * - auth: JWT tokens, login state, user identity, roles, permissions
 * - user: user profile data
 * - roles: system-wide role definitions (for admin management)
 * - permissions: system-wide permission definitions (for admin management)
 * - tips: tips feed data for subscriber consumption
 * - plans: subscription plans visible to all users
 * 
 * Admin CRUD pages (user management, tip management, plan management) use DIRECT service
 * calls with component-local signals. This is intentional:
 * - These pages have isolated state not shared across components
 * - They benefit from simpler, more direct data flow
 * - No stale cache concerns on admin write operations
 * 
 * Pattern: Use NgRx store for shared read-heavy state; use direct service calls for 
 * isolated admin CRUD views.
 */
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
