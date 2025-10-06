
import { Component, OnInit, inject } from '@angular/core';
import { OrderService, Order, ListOrdersResponse } from '../order.service';
import { GlobalLoadingService } from '../../../shared/global-loading.service';
import { ToastService } from '../../../shared/toast.service';

@Component({
  selector: 'app-order-list',
  templateUrl: './order-list.component.html',
  styleUrls: ['./order-list.component.css']
})
export class OrderListComponent implements OnInit {
  orders: Order[] = [];
  sortActive = 'createdAt';
  sortDirection: 'asc' | 'desc' = 'desc';
  displayedColumns: string[] = ['id', 'customerName', 'status', 'createdAt'];
  totalCount = 0;
  page = 1;
  pageSize = 10;
  loading = false;
  showCreateOrderModal = false;

  orderService = inject(OrderService);
  globalLoading = inject(GlobalLoadingService);
  toastService = inject(ToastService);

  // Expose Math for template
  Math = Math;

  // Create a simple data source object
  get dataSource() {
    return { data: this.orders };
  }

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading = true;
    this.globalLoading.show();

    this.orderService.listOrders(this.page, this.pageSize, this.sortActive, this.sortDirection === 'desc').subscribe({
      next: (res: ListOrdersResponse) => {
        this.orders = res.orders;
        this.totalCount = res.totalCount;
        this.loading = false;
        this.globalLoading.hide();
      },
      error: (error) => {
        this.loading = false;
        this.globalLoading.hide();
        let errorMessage = 'Failed to load orders. Please try again.';

        if (error.status === 401) {
          errorMessage = 'Unauthorized. Please login again.';
        } else if (error.status === 0) {
          errorMessage = 'Cannot connect to server. Please check if the API is running.';
        }

        this.toastService.error(errorMessage);
      }
    });
  }

  onSort(column: string): void {
    if (this.sortActive === column) {
      // Toggle direction for same column
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      // Set new column with appropriate default direction
      this.sortActive = column;
      // For datetime fields, default to desc (newest first)
      // For other fields, default to asc
      this.sortDirection = (column === 'createdAt') ? 'desc' : 'asc';
    }
    this.page = 1; // Reset to first page when sorting
    this.loadOrders();
  }

  onPageSizeChange(): void {
    this.page = 1; // Reset to first page when page size changes
    this.loadOrders();
  }

  // Pagination methods
  nextPage(): void {
    if (this.page * this.pageSize < this.totalCount) {
      this.page++;
      this.loadOrders();
    }
  }

  previousPage(): void {
    if (this.page > 1) {
      this.page--;
      this.loadOrders();
    }
  }

  openCreateOrderModal(): void {
    this.showCreateOrderModal = true;
  }

  closeCreateOrderModal(): void {
    this.showCreateOrderModal = false;
  }

  onOrderPlaced(): void {
    this.closeCreateOrderModal();
    this.loadOrders();
    this.toastService.success('Order placed successfully!');
  }

  trackByOrderId(index: number, order: Order): string {
    return order.id;
  }

  changeStatus(order: Order, newStatus: string): void {
    if (!newStatus || newStatus === order.status) return;
    this.globalLoading.show();
    this.orderService.updateOrderStatus(order.id, newStatus).subscribe({
      next: () => {
        order.status = newStatus;
        this.globalLoading.hide();
        this.toastService.success('Order status updated successfully!');
      },
      error: (error) => {
        this.globalLoading.hide();
        let errorMessage = 'Failed to update order status.';

        if (error.status === 400 && error.error?.errors) {
          const validationErrors = Object.values(error.error.errors).flat();
          errorMessage = `Validation failed: ${validationErrors.join(', ')}`;
        } else if (error.status === 401) {
          errorMessage = 'Unauthorized. Please login again.';
        } else if (error.status === 404) {
          errorMessage = 'Order not found.';
        }

        this.toastService.error(errorMessage);
      }
    });
  }
}