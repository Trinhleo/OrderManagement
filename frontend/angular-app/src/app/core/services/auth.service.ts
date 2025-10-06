import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, BehaviorSubject, tap, of, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LoginRequest {
    username: string;
    password: string;
}

export interface LoginResponse {
    token: string;
    username: string;
    roles: string[];
}

export interface User {
    username: string;
    token: string;
    expiresAt: Date;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
    private readonly http = inject(HttpClient);
    private readonly router = inject(Router);
    private readonly apiUrl = `${environment.apiUrl}/Auth`; // Note: Capital A for Auth

    // Modern Angular 17 signals
    private readonly currentUserSignal = signal<User | null>(null);

    // Computed properties using signals
    readonly isAuthenticated = computed(() => {
        const user = this.currentUserSignal();
        return !!user && new Date() < user.expiresAt;
    });

    readonly currentUser = computed(() => this.currentUserSignal());

    constructor() {
        // Load user from localStorage on service initialization
        this.loadUserFromStorage();
    }

    login(credentials: LoginRequest): Observable<LoginResponse> {
        console.log('AuthService.login() called with:', { username: credentials.username });
        console.log('Making request to:', `${this.apiUrl}/login`);

        return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials).pipe(
            tap(response => {
                console.log('Login response received:', response);

                // Parse JWT token to get expiration time
                const tokenExpirationTime = this.getTokenExpiration(response.token);

                const user: User = {
                    username: response.username,
                    token: response.token,
                    expiresAt: tokenExpirationTime
                };

                this.setCurrentUser(user);
                this.saveUserToStorage(user);
                console.log('User saved to storage:', user);
            })
        );
    }

    logout(): void {
        this.currentUserSignal.set(null);
        this.removeUserFromStorage();
        this.router.navigate(['/auth/login']);
    }

    refreshToken(): Observable<LoginResponse | null> {
        const user = this.currentUserSignal();
        if (!user) {
            return of(null);
        }

        // Check if token is close to expiry (refresh if expires within 5 minutes)
        const fiveMinutesFromNow = new Date(Date.now() + 5 * 60 * 1000);
        if (user.expiresAt > fiveMinutesFromNow) {
            return of(null); // Token is still valid
        }

        // In a real app, you'd have a refresh endpoint
        // For demo purposes, we'll just logout on expiry
        this.logout();
        return throwError(() => new Error('Token expired'));
    }

    getToken(): string | null {
        const user = this.currentUserSignal();
        return user?.token || null;
    }

    private getTokenExpiration(token: string): Date {
        try {
            // Decode JWT token to get expiration
            const payload = JSON.parse(atob(token.split('.')[1]));
            // JWT exp is in seconds, convert to milliseconds
            return new Date(payload.exp * 1000);
        } catch (error) {
            console.error('Error parsing JWT token:', error);
            // Default to 1 hour from now if parsing fails
            return new Date(Date.now() + 60 * 60 * 1000);
        }
    }

    private setCurrentUser(user: User): void {
        this.currentUserSignal.set(user);
    }

    private saveUserToStorage(user: User): void {
        const userToSave = {
            username: user.username,
            token: user.token,
            expiresAt: user.expiresAt.toISOString() // Convert Date to ISO string
        };
        console.log('Saving user to localStorage:', userToSave);
        localStorage.setItem('currentUser', JSON.stringify(userToSave));
    }

    private loadUserFromStorage(): void {
        const storedUser = localStorage.getItem('currentUser');
        if (storedUser) {
            try {
                const userData = JSON.parse(storedUser);
                const user: User = {
                    username: userData.username,
                    token: userData.token,
                    expiresAt: new Date(userData.expiresAt)
                };

                // Check if token is still valid
                if (new Date() < user.expiresAt) {
                    this.setCurrentUser(user);
                } else {
                    this.removeUserFromStorage();
                }
            } catch (error) {
                console.error('Error loading user from storage:', error);
                this.removeUserFromStorage();
            }
        }
    }

    private removeUserFromStorage(): void {
        localStorage.removeItem('currentUser');
    }
}