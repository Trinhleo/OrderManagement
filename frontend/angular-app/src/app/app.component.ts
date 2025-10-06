import { Component, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './core/services/auth.service';
import { CleanLoginComponent } from './features/auth/login/clean-login.component';
import { ToastComponent } from './shared/toast.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    CleanLoginComponent,
    ToastComponent
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  // Expose auth state to template
  readonly isAuthenticated = this.authService.isAuthenticated;
  readonly currentUser = this.authService.currentUser;

  logout() {
    this.authService.logout();
  }
}
