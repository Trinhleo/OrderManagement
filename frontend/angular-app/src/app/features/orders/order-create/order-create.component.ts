

import { Component, Output, EventEmitter } from '@angular/core';
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
  lines = [{ product: '', quantity: 1, price: 1, currency: 'USD' }];
  loading = false;


  @Output() orderPlaced = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();

  constructor(
    private orderService: OrderService,
    private snackBar: MatSnackBar,
    private globalLoading: GlobalLoadingService
  ) { }

  addLine() {
    this.lines.push({ product: '', quantity: 1, price: 1, currency: 'USD' });
  }

  removeLine(i: number) {
    this.lines.splice(i, 1);
  }

  place() {
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
        next: res => {
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
