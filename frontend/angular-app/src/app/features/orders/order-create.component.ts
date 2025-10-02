import { Component } from '@angular/core';
import { OrderService } from './order.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-order-create',
  template: `
    <mat-toolbar color="primary">
      <span>Create Order</span>
    </mat-toolbar>

    <div class="container mat-elevation-z4">
      <form (ngSubmit)="place()" #orderForm="ngForm" autocomplete="off">
        <!-- Customer -->
        <mat-form-field appearance="fill" class="full-width">
          <mat-label>Customer Name</mat-label>
          <input matInput name="customerName" [(ngModel)]="customerName" required />
        </mat-form-field>

        <!-- Order Lines -->
        <div *ngFor="let line of lines; let i = index" class="order-line">
          <mat-form-field appearance="fill" class="flex-2">
            <mat-label>Product</mat-label>
            <input matInput [(ngModel)]="line.product" name="product{{i}}" required />
          </mat-form-field>

          <mat-form-field appearance="fill" class="flex-1">
            <mat-label>Quantity</mat-label>
            <input matInput type="number" [(ngModel)]="line.quantity" name="quantity{{i}}" required min="1" />
          </mat-form-field>

          <mat-form-field appearance="fill" class="flex-1">
            <mat-label>Price</mat-label>
            <input matInput type="number" [(ngModel)]="line.price" name="price{{i}}" required min="0.01" step="0.01" />
          </mat-form-field>

          <mat-form-field appearance="fill" class="flex-1">
            <mat-label>Currency</mat-label>
            <input matInput [(ngModel)]="line.currency" name="currency{{i}}" required />
          </mat-form-field>

          <button mat-icon-button color="warn" type="button" (click)="removeLine(i)" *ngIf="lines.length > 1">
            <mat-icon>remove_circle</mat-icon>
          </button>
        </div>

        <!-- Add Line -->
        <button mat-button color="primary" type="button" (click)="addLine()">
          <mat-icon>add_circle</mat-icon> Add Line
        </button>

        <!-- Place Order -->
        <div class="actions">
          <button mat-raised-button color="accent" type="submit" [disabled]="orderForm.invalid || loading">
            <ng-container *ngIf="!loading">Place Order</ng-container>
            <mat-progress-spinner *ngIf="loading" mode="indeterminate" diameter="20"></mat-progress-spinner>
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [`
    .container {
      margin: 32px auto;
      padding: 24px;
      background: #fff;
      border-radius: 8px;
      max-width: 700px;
    }
    .order-line {
      display: flex;
      gap: 12px;
      align-items: center;
      margin-bottom: 12px;
    }
    .flex-1 { flex: 1; }
    .flex-2 { flex: 2; }
    .full-width { width: 100%; }
    .actions {
      margin-top: 24px;
      display: flex;
      justify-content: flex-end;
    }
  `]
})
export class OrderCreateComponent {
  customerName = '';
  lines = [{ product: '', quantity: 1, price: 0, currency: 'USD' }];
  loading = false;

  constructor(private orderService: OrderService, private snackBar: MatSnackBar) { }

  addLine() {
    this.lines.push({ product: '', quantity: 1, price: 0, currency: 'USD' });
  }

  removeLine(i: number) {
    this.lines.splice(i, 1);
  }

  place() {
    if (!this.customerName || this.lines.some(l => !l.product || !l.quantity || !l.price || !l.currency)) return;
    this.loading = true;

    const line = this.lines[0];
    this.orderService.placeOrder(line.product, line.quantity, line.price, line.currency, this.customerName)
      .subscribe({
        next: res => {
          this.loading = false;
          this.snackBar.open(`Order placed! ID: ${res.orderId}`, 'Close', { duration: 3000 });
        },
        error: () => {
          this.loading = false;
          this.snackBar.open('Failed to place order. Please try again.', 'Close', { duration: 3000 });
        }
      });
  }
}
