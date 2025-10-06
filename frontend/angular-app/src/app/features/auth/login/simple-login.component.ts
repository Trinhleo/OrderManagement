import { Component, inject, signal } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/toast.service';

@Component({
    selector: 'app-simple-login',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule
    ],
    template: `
        <div class="login-container">
            <div class="login-card">
                <div class="header">
                    <h1>Order Management System</h1>
                    <p>Sign in to your account</p>
                </div>
                
                <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="login-form">
                    <div class="form-group">
                        <label for="username">Username</label>
                        <input 
                            id="username"
                            type="text" 
                            formControlName="username" 
                            required
                            placeholder="Enter your username">
                        <div *ngIf="loginForm.get('username')?.invalid && loginForm.get('username')?.touched" class="error">
                            Username is required
                        </div>
                    </div>

                    <div class="form-group">
                        <label for="password">Password</label>
                        <div class="password-input-container">
                            <input 
                                id="password"
                                [type]="hidePassword() ? 'password' : 'text'" 
                                formControlName="password" 
                                required
                                placeholder="Enter your password">
                            <button type="button" (click)="togglePasswordVisibility()" class="password-toggle">
                                {{ hidePassword() ? '👁️' : '🙈' }}
                            </button>
                        </div>
                        <div *ngIf="loginForm.get('password')?.invalid && loginForm.get('password')?.touched" class="error">
                            Password is required
                        </div>
                    </div>

                    <button 
                        type="submit" 
                        [disabled]="loginForm.invalid || isLoading()"
                        class="submit-button">
                        <span *ngIf="isLoading()" class="spinner">⏳</span>
                        {{ isLoading() ? 'Signing in...' : 'Sign In' }}
                    </button>
                </form>
            </div>
        </div>
    `,
    styles: [`
        .login-container {
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 20px;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }

        .login-card {
            background: white;
            border-radius: 12px;
            box-shadow: 0 10px 25px rgba(0,0,0,0.2);
            padding: 40px;
            width: 100%;
            max-width: 400px;
        }

        .header {
            text-align: center;
            margin-bottom: 30px;
        }

        .header h1 {
            color: #333;
            font-size: 28px;
            font-weight: 600;
            margin: 0 0 10px 0;
        }

        .header p {
            color: #666;
            font-size: 16px;
            margin: 0;
        }

        .login-form {
            width: 100%;
        }

        .form-group {
            margin-bottom: 20px;
        }

        .form-group label {
            display: block;
            margin-bottom: 8px;
            font-weight: 500;
            color: #333;
            font-size: 14px;
        }

        .form-group input {
            width: 100%;
            padding: 12px 16px;
            border: 2px solid #e1e5e9;
            border-radius: 8px;
            font-size: 16px;
            transition: border-color 0.3s ease;
            box-sizing: border-box;
        }

        .form-group input:focus {
            outline: none;
            border-color: #667eea;
            box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
        }

        .password-input-container {
            position: relative;
            display: flex;
            align-items: center;
        }

        .password-input-container input {
            padding-right: 50px;
        }

        .password-toggle {
            position: absolute;
            right: 12px;
            background: none;
            border: none;
            cursor: pointer;
            font-size: 18px;
            padding: 4px;
            border-radius: 4px;
            transition: background-color 0.2s ease;
        }

        .password-toggle:hover {
            background-color: #f0f0f0;
        }

        .error {
            color: #e74c3c;
            font-size: 14px;
            margin-top: 5px;
        }

        .submit-button {
            width: 100%;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border: none;
            padding: 14px 20px;
            border-radius: 8px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            transition: transform 0.2s ease, box-shadow 0.2s ease;
            margin-top: 10px;
        }

        .submit-button:hover:not(:disabled) {
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(102, 126, 234, 0.4);
        }

        .submit-button:disabled {
            opacity: 0.7;
            cursor: not-allowed;
            transform: none;
        }

        .spinner {
            display: inline-block;
            margin-right: 8px;
            animation: spin 2s linear infinite;
        }

        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }

        @media (max-width: 480px) {
            .login-card {
                padding: 30px 20px;
                margin: 10px;
            }

            .header h1 {
                font-size: 24px;
            }
        }
    `]
})
export class SimpleLoginComponent {
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

            this.authService.login(credentials).subscribe({
                next: () => {
                    this.isLoading.set(false);
                    const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/orders';
                    this.router.navigate([returnUrl]);
                    this.toastService.success('Login successful!');
                },
                error: (error) => {
                    this.isLoading.set(false);
                    let errorMessage = 'Login failed. Please try again.';

                    if (error.status === 401) {
                        errorMessage = 'Invalid username or password.';
                    } else if (error.status === 0) {
                        errorMessage = 'Cannot connect to server. Please check if the API is running.';
                    }

                    this.toastService.error(errorMessage);
                }
            });
        }
    }

    togglePasswordVisibility(): void {
        this.hidePassword.update(value => !value);
    }
}