import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    console.log('Auth guard triggered for:', state.url);
    console.log('Current user:', authService.currentUser());
    console.log('Is authenticated:', authService.isAuthenticated());

    if (authService.isAuthenticated()) {
        console.log('User is authenticated, allowing access');
        return true;
    }

    // Not authenticated, redirect to login page
    console.log('User not authenticated, redirecting to login');
    router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
    return false;
};