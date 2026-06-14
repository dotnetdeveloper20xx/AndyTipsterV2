import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { UserProfile } from './user.state';

export const UserActions = createActionGroup({
  source: 'User',
  events: {
    'Load User': emptyProps(),
    'Load User Success': props<{ profile: UserProfile }>(),
    'Load User Failure': props<{ error: string }>(),

    'Update Profile': props<{ displayName?: string; bio?: string; timezone?: string }>(),
    'Update Profile Success': props<{ profile: UserProfile }>(),
    'Update Profile Failure': props<{ error: string }>(),

    'Upload Avatar': props<{ file: File }>(),
    'Upload Avatar Success': props<{ avatarUrl: string }>(),
    'Upload Avatar Failure': props<{ error: string }>(),

    'Clear User': emptyProps(),
    'Clear Error': emptyProps(),
  },
});
