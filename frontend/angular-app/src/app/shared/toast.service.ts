import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface Toast {
    id: string;
    message: string;
    type: 'success' | 'error' | 'warning' | 'info';
    duration?: number;
}

@Injectable({
    providedIn: 'root'
})
export class ToastService {
    private toastsSubject = new BehaviorSubject<Toast[]>([]);
    public toasts$ = this.toastsSubject.asObservable();

    private toasts: Toast[] = [];

    show(message: string, type: 'success' | 'error' | 'warning' | 'info' = 'info', duration = 5000): void {
        const toast: Toast = {
            id: this.generateId(),
            message,
            type,
            duration
        };

        this.toasts.push(toast);
        this.toastsSubject.next([...this.toasts]);

        // Auto remove toast after duration
        if (duration > 0) {
            setTimeout(() => {
                this.remove(toast.id);
            }, duration);
        }
    }

    success(message: string, duration?: number): void {
        this.show(message, 'success', duration);
    }

    error(message: string, duration?: number): void {
        this.show(message, 'error', duration);
    }

    warning(message: string, duration?: number): void {
        this.show(message, 'warning', duration);
    }

    info(message: string, duration?: number): void {
        this.show(message, 'info', duration);
    }

    remove(id: string): void {
        this.toasts = this.toasts.filter(toast => toast.id !== id);
        this.toastsSubject.next([...this.toasts]);
    }

    clear(): void {
        this.toasts = [];
        this.toastsSubject.next([]);
    }

    private generateId(): string {
        return Math.random().toString(36).substring(2, 9);
    }
}