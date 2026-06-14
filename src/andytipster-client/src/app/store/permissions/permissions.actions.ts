import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Permission } from './permissions.state';

export const PermissionsActions = createActionGroup({
  source: 'Permissions',
  events: {
    'Load Permissions': emptyProps(),
    'Load Permissions Success': props<{ permissions: Permission[] }>(),
    'Load Permissions Failure': props<{ error: string }>(),

    'Load User Permissions': emptyProps(),
    'Load User Permissions Success': props<{ permissions: string[] }>(),
    'Load User Permissions Failure': props<{ error: string }>(),

    'Clear Permissions': emptyProps(),
  },
});
