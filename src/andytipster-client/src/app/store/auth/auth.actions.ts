import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { AuthUser } from './auth.state';

export const AuthActions = createActionGroup({
  source: 'Auth',
  events: {
    'Login': props<{ email: string; password: string }>(),
    'Login Success': props<{ accessToken: string; refreshToken: string; expiresAt: string; user?: AuthUser; roles?: string[]; permissions?: string[] }>(),
    'Login Failure': props<{ error: string }>(),
    'Login Requires 2FA': props<{ email: string }>(),

    'Verify 2FA': props<{ email: string; code: string }>(),
    'Verify 2FA Success': props<{ accessToken: string; refreshToken: string; expiresAt: string; user?: AuthUser; roles?: string[]; permissions?: string[] }>(),
    'Verify 2FA Failure': props<{ error: string }>(),

    'Verify Recovery Code': props<{ email: string; code: string }>(),
    'Verify Recovery Code Success': props<{ accessToken: string; refreshToken: string; expiresAt: string; user?: AuthUser; roles?: string[]; permissions?: string[] }>(),
    'Verify Recovery Code Failure': props<{ error: string }>(),

    'Register': props<{ email: string; password: string; displayName: string }>(),
    'Register Success': emptyProps(),
    'Register Failure': props<{ error: string }>(),

    'Forgot Password': props<{ email: string }>(),
    'Forgot Password Success': emptyProps(),
    'Forgot Password Failure': props<{ error: string }>(),

    'Social Login': props<{ provider: string; accessToken: string }>(),

    'Logout': emptyProps(),
    'Logout Success': emptyProps(),

    'Refresh Token': emptyProps(),
    'Refresh Token Success': props<{ accessToken: string; refreshToken: string; expiresAt: string; user?: AuthUser; roles?: string[]; permissions?: string[] }>(),
    'Refresh Token Failure': emptyProps(),

    'Init Auth': emptyProps(),

    'Clear Error': emptyProps(),
  },
});
