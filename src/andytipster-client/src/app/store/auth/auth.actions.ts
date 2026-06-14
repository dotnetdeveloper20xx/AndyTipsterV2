import { createActionGroup, emptyProps, props } from '@ngrx/store';

export const AuthActions = createActionGroup({
  source: 'Auth',
  events: {
    'Login': props<{ email: string; password: string }>(),
    'Login Success': props<{ accessToken: string; refreshToken: string; expiresAt: number }>(),
    'Login Failure': props<{ error: string }>(),
    'Login Requires 2FA': emptyProps(),

    'Verify 2FA': props<{ code: string }>(),
    'Verify 2FA Success': props<{ accessToken: string; refreshToken: string; expiresAt: number }>(),
    'Verify 2FA Failure': props<{ error: string }>(),

    'Register': props<{ email: string; password: string; displayName: string }>(),
    'Register Success': emptyProps(),
    'Register Failure': props<{ error: string }>(),

    'Logout': emptyProps(),
    'Logout Success': emptyProps(),

    'Refresh Token': emptyProps(),
    'Refresh Token Success': props<{ accessToken: string; refreshToken: string; expiresAt: number }>(),
    'Refresh Token Failure': emptyProps(),

    'Clear Error': emptyProps(),
  },
});
