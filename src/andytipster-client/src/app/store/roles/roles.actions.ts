import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Role } from './roles.state';

export const RolesActions = createActionGroup({
  source: 'Roles',
  events: {
    'Load Roles': emptyProps(),
    'Load Roles Success': props<{ roles: Role[] }>(),
    'Load Roles Failure': props<{ error: string }>(),

    'Load User Roles': emptyProps(),
    'Load User Roles Success': props<{ roleIds: string[] }>(),
    'Load User Roles Failure': props<{ error: string }>(),

    'Create Role': props<{ name: string; hierarchyLevel: number; permissions: string[] }>(),
    'Create Role Success': props<{ role: Role }>(),
    'Create Role Failure': props<{ error: string }>(),

    'Delete Role': props<{ roleId: string }>(),
    'Delete Role Success': props<{ roleId: string }>(),
    'Delete Role Failure': props<{ error: string }>(),

    'Clear Roles': emptyProps(),
  },
});
