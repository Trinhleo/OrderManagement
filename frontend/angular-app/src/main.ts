import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { importProvidersFrom } from '@angular/core';
import { MatSnackBarModule } from '@angular/material/snack-bar';

import { AppComponent } from './app/app.component';
import { routes } from './app/app-routing.module';
import { jwtInterceptor } from './app/core/interceptors/jwt.interceptor';
import { errorInterceptor } from './app/core/interceptors/error.interceptor';
import { debugInterceptor } from './app/core/interceptors/debug.interceptor';

bootstrapApplication(AppComponent, {
    providers: [
        provideRouter(routes),
        provideAnimations(),
        provideHttpClient(
            withInterceptors([debugInterceptor, jwtInterceptor, errorInterceptor])
        ),
        importProvidersFrom(MatSnackBarModule)
    ]
}).catch(err => console.error(err));
