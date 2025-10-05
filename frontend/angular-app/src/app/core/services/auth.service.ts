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
    expiresAt: string;
    username: string;
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
    private readonly apiUrl = `${environment.apiUrl}/auth`;

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
        return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials).pipe(
            tap(response => {
                const user: User = {
                    username: response.username,
                    token: response.token,
                    expiresAt: new Date(response.expiresAt)
                };

                this.setCurrentUser(user);
                this.saveUserToStorage(user);
            })
        );
    }

    logout(): void {
        this.currentUserSignal.set(null);
        this.removeUserFromStorage();
        this.router.navigate(['/login']);
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

    private setCurrentUser(user: User): void {
        this.currentUserSignal.set(user);
    }

    private saveUserToStorage(user: User): void {
        localStorage.setItem('currentUser', JSON.stringify({
            username: user.username,
            token: user.token,
            expiresAt: user.expiresAt.toISOString()
        }));
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