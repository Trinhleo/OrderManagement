
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule, provideHttpClient, withInterceptors } from '@angular/common/http';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';

import { AppComponent } from './app.component';
import { GlobalSpinnerComponent } from './shared/global-spinner.component';
import { AppRoutingModule } from './app-routing.module';
import { jwtInterceptor } from './core/interceptors/jwt.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';

@NgModule({
    declarations: [AppComponent, GlobalSpinnerComponent],
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        HttpClientModule,
        AppRoutingModule,
        MatProgressSpinnerModule,
        MatSnackBarModule
    ],
    providers: [
        provideHttpClient(
            withInterceptors([jwtInterceptor, errorInterceptor])
        )
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
