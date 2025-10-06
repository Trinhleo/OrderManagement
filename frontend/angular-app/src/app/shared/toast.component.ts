import { Component, inject, OnDestroy } from '@angular/core';
import { ToastService, Toast } from './toast.service';
import { Subscription } from 'rxjs';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-toast',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div class="toast-container">
      <div *ngFor="let toast of toasts" 
           class="toast" 
           [class]="'toast-' + toast.type"
           (click)="remove(toast.id)"
           (keyup.enter)="remove(toast.id)"
           (keyup.space)="remove(toast.id)"
           tabindex="0"
           role="button"
           [attr.aria-label]="'Dismiss ' + toast.type + ' notification'">
        <div class="toast-content">
          <div class="toast-icon">
            <span *ngIf="toast.type === 'success'">✓</span>
            <span *ngIf="toast.type === 'error'">✕</span>
            <span *ngIf="toast.type === 'warning'">⚠</span>
            <span *ngIf="toast.type === 'info'">ℹ</span>
          </div>
          <div class="toast-message">{{ toast.message }}</div>
          <button class="toast-close" (click)="remove(toast.id)" type="button">
            ×
          </button>
        </div>
      </div>
    </div>
  `,
    styles: [`
    .toast-container {
      position: fixed;
      top: 20px;
      right: 20px;
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: 8px;
      max-width: 400px;
    }

    .toast {
      background: white;
      border-radius: 8px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
      border-left: 4px solid;
      cursor: pointer;
      animation: slideIn 0.3s ease-out;
      transition: all 0.2s ease;
    }

    .toast:hover {
      transform: translateX(-4px);
      box-shadow: 0 6px 16px rgba(0, 0, 0, 0.2);
    }

    .toast-success {
      border-left-color: #10b981;
    }

    .toast-error {
      border-left-color: #ef4444;
    }

    .toast-warning {
      border-left-color: #f59e0b;
    }

    .toast-info {
      border-left-color: #3b82f6;
    }

    .toast-content {
      display: flex;
      align-items: center;
      padding: 12px 16px;
      gap: 12px;
    }

    .toast-icon {
      width: 20px;
      height: 20px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: bold;
      font-size: 14px;
      flex-shrink: 0;
    }

    .toast-success .toast-icon {
      background: #dcfce7;
      color: #16a34a;
    }

    .toast-error .toast-icon {
      background: #fef2f2;
      color: #dc2626;
    }

    .toast-warning .toast-icon {
      background: #fef3c7;
      color: #d97706;
    }

    .toast-info .toast-icon {
      background: #dbeafe;
      color: #2563eb;
    }

    .toast-message {
      flex: 1;
      font-size: 14px;
      color: #374151;
      line-height: 1.4;
    }

    .toast-close {
      background: none;
      border: none;
      font-size: 18px;
      color: #9ca3af;
      cursor: pointer;
      padding: 0;
      width: 20px;
      height: 20px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 50%;
      transition: all 0.2s ease;
    }

    .toast-close:hover {
      background: #f3f4f6;
      color: #6b7280;
    }

    @keyframes slideIn {
      from {
        transform: translateX(100%);
        opacity: 0;
      }
      to {
        transform: translateX(0);
        opacity: 1;
      }
    }

    @media (max-width: 480px) {
      .toast-container {
        right: 16px;
        left: 16px;
        max-width: none;
      }
    }
  `]
})
export class ToastComponent implements OnDestroy {
    toasts: Toast[] = [];
    private subscription: Subscription;

    private toastService = inject(ToastService);

    constructor() {
        this.subscription = this.toastService.toasts$.subscribe(toasts => {
            this.toasts = toasts;
        });
    }

    ngOnDestroy(): void {
        this.subscription.unsubscribe();
    }

    remove(id: string): void {
        this.toastService.remove(id);
    }
}