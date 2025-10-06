import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ApiTestComponent } from '../../../components/api-test.component';
import { AuthDebugComponent } from '../../../components/auth-debug.component';
import { ToastService } from '../../../shared/toast.service';

@Component({
    selector: 'app-clean-login',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ApiTestComponent,
        AuthDebugComponent
    ],
    template: `
        <div class="login-wrapper">
            <div class="login-container">
                <div class="login-header">
                    <h1>Order Management System</h1>
                    <p>Please sign in to continue</p>
                </div>
                
                <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="login-form">
                    <div class="input-group">
                        <label>Username *</label>
                        <input 
                            type="text" 
                            formControlName="username" 
                            placeholder="Enter username"
                            class="form-input">
                        <div *ngIf="loginForm.get('username')?.invalid && loginForm.get('username')?.touched" class="error-text">
                            Username is required
                        </div>
                    </div>

                    <div class="input-group">
                        <label>Password *</label>
                        <div class="password-wrapper">
                            <input 
                                [type]="hidePassword() ? 'password' : 'text'" 
                                formControlName="password" 
                                placeholder="Enter password"
                                class="form-input password-input">
                            <button type="button" (click)="togglePasswordVisibility()" class="toggle-btn">
                                {{ hidePassword() ? '👁️' : '🙈' }}
                            </button>
                        </div>
                        <div *ngIf="loginForm.get('password')?.invalid && loginForm.get('password')?.touched" class="error-text">
                            Password is required
                        </div>
                    </div>

                    <button 
                        type="submit" 
                        [disabled]="loginForm.invalid || isLoading()"
                        class="login-btn">
                        <span *ngIf="isLoading()" class="loading-icon">⏳</span>
                        {{ isLoading() ? 'Signing in...' : 'Sign In' }}
                    </button>
                </form>
            </div>
        </div>
    `,
    styles: [`
        .login-wrapper {
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 20px;
        }

        .login-container {
            background: #ffffff;
            border-radius: 12px;
            box-shadow: 0 20px 40px rgba(0, 0, 0, 0.15);
            padding: 48px;
            width: 100%;
            max-width: 420px;
        }

        .login-header {
            text-align: center;
            margin-bottom: 32px;
        }

        .login-header h1 {
            color: #1a202c;
            font-size: 28px;
            font-weight: 700;
            margin: 0 0 12px 0;
            letter-spacing: -0.025em;
        }

        .login-header p {
            color: #718096;
            font-size: 16px;
            margin: 0;
            font-weight: 400;
        }

        .login-form {
            width: 100%;
        }

        .input-group {
            margin-bottom: 24px;
        }

        .input-group label {
            display: block;
            margin-bottom: 8px;
            font-weight: 600;
            color: #2d3748;
            font-size: 14px;
            letter-spacing: 0.025em;
        }

        .form-input {
            width: 100%;
            padding: 14px 16px;
            border: 2px solid #e2e8f0;
            border-radius: 8px;
            font-size: 16px;
            background-color: #ffffff;
            transition: all 0.2s ease;
            color: #2d3748;
            box-sizing: border-box;
        }

        .form-input:focus {
            border-color: #667eea;
            box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
            outline: none;
        }

        .form-input::placeholder {
            color: #a0aec0;
        }

        .password-wrapper {
            position: relative;
        }

        .password-input {
            padding-right: 50px;
        }

        .toggle-btn {
            position: absolute;
            right: 12px;
            top: 50%;
            transform: translateY(-50%);
            background: none;
            border: none;
            cursor: pointer;
            font-size: 18px;
            padding: 6px;
            border-radius: 4px;
            transition: background-color 0.2s ease;
        }

        .toggle-btn:hover {
            background-color: #f7fafc;
        }

        .error-text {
            color: #e53e3e;
            font-size: 13px;
            margin-top: 6px;
            font-weight: 500;
        }

        .login-btn {
            width: 100%;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: #ffffff;
            border: none;
            padding: 16px 24px;
            border-radius: 8px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.2s ease;
            margin-top: 8px;
            letter-spacing: 0.025em;
        }

        .login-btn:hover:not(:disabled) {
            transform: translateY(-1px);
            box-shadow: 0 8px 25px rgba(102, 126, 234, 0.3);
        }

        .login-btn:disabled {
            opacity: 0.6;
            cursor: not-allowed;
            transform: none;
        }

        .loading-icon {
            display: inline-block;
            margin-right: 8px;
            animation: spin 2s linear infinite;
        }

        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }

        @media (max-width: 480px) {
            .login-container {
                padding: 32px 24px;
                margin: 16px;
            }

            .login-header h1 {
                font-size: 24px;
            }

            .login-header p {
                font-size: 14px;
            }
        }
    `]
})
export class CleanLoginComponent {
    private readonly fb = inject(FormBuilder);
    private readonly authService = inject(AuthService);
    private readonly router = inject(Router);
    private readonly route = inject(ActivatedRoute);
    private readonly toastService = inject(ToastService);

    readonly hidePassword = signal(true);
    readonly isLoading = signal(false);

    readonly loginForm = this.fb.group({
        username: ['admin', [Validators.required]],
        password: ['admin123', [Validators.required]]
    });

    onSubmit(): void {
        if (this.loginForm.valid) {
            this.isLoading.set(true);

            const credentials = this.loginForm.value as { username: string; password: string };

            console.log('Attempting login with:', { username: credentials.username });
            console.log('API URL:', `https://localhost:56642/api/auth/login`);

            this.authService.login(credentials).subscribe({
                next: (response) => {
                    console.log('Login successful:', response);
                    console.log('Current user after login:', this.authService.currentUser());
                    console.log('Is authenticated:', this.authService.isAuthenticated());

                    this.isLoading.set(false);
                    const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/orders';
                    console.log('Navigating to:', returnUrl);

                    // Add a small delay to ensure localStorage is saved
                    setTimeout(() => {
                        console.log('localStorage contents:', localStorage.getItem('currentUser'));
                        this.router.navigate([returnUrl]).then(success => {
                            console.log('Navigation success:', success);
                            if (success) {
                                console.log('Successfully navigated to dashboard');
                            } else {
                                console.error('Navigation failed');
                            }
                        });
                    }, 100);
                },
                error: (error) => {
                    console.error('Login error:', error);
                    this.isLoading.set(false);
                    let errorMessage = 'Login failed. Please try again.';

                    if (error.status === 401) {
                        errorMessage = 'Invalid username or password.';
                    } else if (error.status === 0) {
                        errorMessage = 'Cannot connect to server. Please check if the API is running and CORS is enabled.';
                    } else if (error.status === 403) {
                        errorMessage = 'Access forbidden.';
                    } else if (error.status >= 500) {
                        errorMessage = 'Server error. Please try again later.';
                    } else {
                        errorMessage = `Error ${error.status}: ${error.message || 'Unknown error'}`;
                    }

                    this.toastService.error(`${errorMessage}\n\nTechnical details: ${JSON.stringify(error)}`);
                }
            });
        }
    }

    togglePasswordVisibility(): void {
        this.hidePassword.update(value => !value);
    }
}