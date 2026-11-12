import { EntraAuthService, getRuntimeMsalInstance } from './core/auth/entra-auth.service';
import { AuthService } from './core/auth/auth.service';
import { ApplicationConfig, inject, provideAppInitializer, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { apiErrorInterceptor, authInterceptor } from './core/http/interceptors';
import { MSAL_INSTANCE, MsalBroadcastService } from '@azure/msal-angular';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    // EntraAuthService creates the single runtime MSAL instance after loading
    // /api/auth/local-config. The broadcast service itself must still live in
    // the root injector for standalone components and guards.
    { provide: MSAL_INSTANCE, useFactory: getRuntimeMsalInstance },
    MsalBroadcastService,
    provideAppInitializer(() => {const entra=inject(EntraAuthService), auth=inject(AuthService);return entra.initialize().then(()=>auth.initialize());}),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor, apiErrorInterceptor]))
  ]
};
