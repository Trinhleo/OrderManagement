

import { Component, Output, EventEmitter, inject } from '@angular/core';
import { OrderService, OrderLine } from '../order.service';
import { GlobalLoadingService } from '../../../shared/global-loading.service';
import { ToastService } from '../../../shared/toast.service';

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
  toastService = inject(ToastService);
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

    const command = {
      customerName: this.customerName,
      lines: this.lines.map(l => ({
        product: l.product,
        quantity: l.quantity,
        price: l.price,
        currency: l.currency
      }))
    };

    this.orderService.placeOrder(command)
      .subscribe({
        next: (res) => {
          this.loading = false;
          this.globalLoading.hide();
          this.toastService.success(`Order placed successfully! ID: ${res.orderId}`);
          setTimeout(() => {
            this.orderPlaced.emit();
          }, 2000);
        },
        error: (error) => {
          this.loading = false;
          this.globalLoading.hide();
          let errorMessage = 'Failed to place order. Please try again.';

          if (error.status === 400 && error.error?.errors) {
            // Handle validation errors from FluentValidation
            const validationErrors = Object.values(error.error.errors).flat();
            errorMessage = `Validation failed: ${validationErrors.join(', ')}`;
          } else if (error.status === 401) {
            errorMessage = 'Unauthorized. Please login again.';
          }

          this.toastService.error(errorMessage);
        }
      });
  }
}
