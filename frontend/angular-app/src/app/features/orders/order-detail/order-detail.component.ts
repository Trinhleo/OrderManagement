
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { OrderService } from '../order.service';
import { GlobalLoadingService } from '../../../shared/global-loading.service';

interface Order {
  id: string;
  customerName: string;
  status: string;
  createdAt: string;
  lines: { product: string; quantity: number; price: number; amount?: number; currency: string }[];
  _newStatus?: string;
}

@Component({
  selector: 'app-order-detail',
  templateUrl: './order-detail.component.html',
  styleUrls: ['./order-detail.component.css']
})
export class OrderDetailComponent implements OnInit {
  order: Order | null = null;
  loading = false;

  route = inject(ActivatedRoute);
  orderService = inject(OrderService);
  router = inject(Router);
  location = inject(Location);
  globalLoading = inject(GlobalLoadingService);
  back(): void {
    this.location.back();
  }
  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loading = true;
      this.globalLoading.show();
      this.orderService.getOrder(id).subscribe({
        next: (o: Order) => {
          this.order = o;
          this.loading = false;
          this.globalLoading.hide();
        },
        error: () => {
          this.loading = false;
          this.globalLoading.hide();
        }
      });
    }
  }

  updateStatus(): void {
    if (!this.order) return;
    const newStatus = this.order._newStatus;
    if (!newStatus || newStatus === this.order.status) return;
    this.globalLoading.show();
    this.orderService.updateOrderStatus(this.order.id, newStatus).subscribe({
      next: () => {
        if (this.order) {
          this.order.status = newStatus;
          this.order._newStatus = undefined;
        }
        this.globalLoading.hide();
        // Optionally show a toast/snackbar here
      },
      error: () => {
        this.globalLoading.hide();
        // Optionally show a toast/snackbar here
      }
    });
  }

  getTotalQuantity(): number {
    if (!this.order?.lines) return 0;
    return this.order.lines.reduce((total, line) => total + line.quantity, 0);
  }

  getTotalAmount(): number {
    if (!this.order?.lines) return 0;
    return this.order.lines.reduce((total, line) => total + (line.amount || line.price || 0), 0);
  }
}
