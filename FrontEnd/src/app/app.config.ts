import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import { HTTP_INTERCEPTORS, HttpClientModule, provideHttpClient } from '@angular/common/http';
import { AppTranslateModule } from './shared/modules/app-translate.module';
import { AuthInterceptor } from './shared/interceptors/auth-interceptor';
import { provideAnimations } from '@angular/platform-browser/animations';
import { TokenInterceptor } from './shared/interceptors/token.interceptor';
import { routes } from './app.routes';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeng/themes/aura';
import { provideToastr } from 'ngx-toastr';


export const appConfig: ApplicationConfig = {
  providers: [
    provideAnimations(), // required animations providers
    provideToastr(), // Toastr providers
    provideRouter(routes),
    importProvidersFrom(HttpClientModule),
    importProvidersFrom(AppTranslateModule.forRoot()), 
    provideHttpClient(),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: TokenInterceptor,
      multi: true
    },
    providePrimeNG({
      theme: {
          preset: Aura
      }
  }),
  
  ]
};


