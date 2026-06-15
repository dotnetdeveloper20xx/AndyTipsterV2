import { ApplicationConfig, isDevMode, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { provideServiceWorker } from '@angular/service-worker';

import { routes } from './app.routes';
import { reducers } from './store';
import { AuthEffects } from './store/auth/auth.effects';
import { UserEffects } from './store/user/user.effects';
import { RolesEffects } from './store/roles/roles.effects';
import { PermissionsEffects } from './store/permissions/permissions.effects';
import { TipsEffects } from './store/tips/tips.effects';
import { PlansEffects } from './store/plans/plans.effects';
import { jwtInterceptor } from './core/interceptors/jwt.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withInterceptors([jwtInterceptor])),
    provideAnimationsAsync(),
    provideStore(reducers),
    provideEffects([
      AuthEffects,
      UserEffects,
      RolesEffects,
      PermissionsEffects,
      TipsEffects,
      PlansEffects,
    ]),
    provideStoreDevtools({
      maxAge: 25,
      logOnly: !isDevMode(),
      autoPause: true,
    }),
    provideServiceWorker('ngsw-worker.js', {
      enabled: !isDevMode(),
      registrationStrategy: 'registerWhenStable:30000',
    }),
  ],
};
