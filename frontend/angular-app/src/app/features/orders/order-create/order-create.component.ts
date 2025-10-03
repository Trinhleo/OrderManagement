

import { Component, Output, EventEmitter, inject } from '@angular/core';
import { OrderService, OrderLine } from '../order.service';
import { GlobalLoadingService } from '../../../shared/global-loading.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-order-create',
  templateUrl: './order-create.component.html',
  styleUrls: ['./order-create.component.css']
  // ...existing code...
})
export class OrderCreateComponent {
  customerName = '';
  lines: OrderLine[] = [{ product: '', quantity: 1, price: 1, currency: 'USD' }];
  loading = false;


  @Output() orderPlaced = new EventEmitter<void>();
  @Output() cancelOrder = new EventEmitter<void>();

  orderService = inject(OrderService);
  snackBar = inject(MatSnackBar);
  globalLoading = inject(GlobalLoadingService);

  addLine(): void {
    this.lines.push({ product: '', quantity: 1, price: 1, currency: 'USD' });
  }

  removeLine(i: number): void {
    this.lines.splice(i, 1);
  }

  place(): void {
    if (!this.customerName || this.lines.some(l => !l.product || !l.quantity || !l.price || !l.currency)) return;
    this.loading = true;
    this.globalLoading.show();
    const lines: OrderLine[] = this.lines.map(l => ({
      product: l.product,
      quantity: l.quantity,
      price: l.price,
      currency: l.currency
    }));
    this.orderService.placeOrder(this.customerName, lines)
      .subscribe({
        next: (res: { orderId: string }) => {
          this.loading = false;
          this.globalLoading.hide();
          this.snackBar.open(`Order placed! ID: ${res.orderId}`, 'Close', { duration: 2000 });
          setTimeout(() => {
            this.orderPlaced.emit();
          }, 2000);
        },
        error: () => {
          this.loading = false;
          this.globalLoading.hide();
          this.snackBar.open('Failed to place order. Please try again.', 'Close', { duration: 3000 });
        }
      });
  }
}
