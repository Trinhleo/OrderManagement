import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../core/services/auth.service';
import { Router } from '@angular/router';

@Component({
    selector: 'app-auth-debug',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div style="padding: 20px; background: #f5f5f5; margin: 20px; border-radius: 8px;">
      <h3>🔍 Authentication Debug Panel</h3>
      
      <div style="margin: 15px 0;">
        <strong>Current User:</strong>
        <pre>{{ currentUser() | json }}</pre>
      </div>

      <div style="margin: 15px 0;">
        <strong>Is Authenticated:</strong>
        <span [style.color]="isAuthenticated() ? 'green' : 'red'">
          {{ isAuthenticated() }}
        </span>
      </div>

      <div style="margin: 15px 0;">
        <strong>Token:</strong>
        <div style="word-break: break-all; background: #eee; padding: 10px; border-radius: 4px; font-family: monospace; font-size: 12px;">
          {{ getToken() || 'No token' }}
        </div>
      </div>

      <div style="margin: 15px 0;">
        <strong>LocalStorage:</strong>
        <pre style="background: #eee; padding: 10px; border-radius: 4px;">{{ getLocalStorageData() | json }}</pre>
      </div>

      <div style="margin: 15px 0;">
        <button (click)="testAuth()" style="padding: 10px 20px; background: #007bff; color: white; border: none; border-radius: 4px; cursor: pointer;">
          🔄 Refresh Auth State
        </button>
        
        <button (click)="logout()" style="padding: 10px 20px; background: #dc3545; color: white; border: none; border-radius: 4px; cursor: pointer; margin-left: 10px;">
          🚪 Logout
        </button>

        <button (click)="navigateToOrders()" style="padding: 10px 20px; background: #28a745; color: white; border: none; border-radius: 4px; cursor: pointer; margin-left: 10px;">
          📋 Go to Orders
        </button>
      </div>
    </div>
  `
})
export class AuthDebugComponent {
    private readonly authService = inject(AuthService);
    private readonly router = inject(Router);

    readonly currentUser = this.authService.currentUser;
    readonly isAuthenticated = this.authService.isAuthenticated;

    getToken() {
        return this.authService.getToken();
    }

    getLocalStorageData() {
        const data = localStorage.getItem('currentUser');
        return data ? JSON.parse(data) : null;
    }

    testAuth() {
        console.log('=== AUTH DEBUG ===');
        console.log('Current user:', this.currentUser());
        console.log('Is authenticated:', this.isAuthenticated());
        console.log('Token:', this.getToken());
        console.log('LocalStorage:', this.getLocalStorageData());
    }

    logout() {
        this.authService.logout();
    }

    navigateToOrders() {
        this.router.navigate(['/orders']).then(success => {
            console.log('Navigation to orders success:', success);
        });
    }
}