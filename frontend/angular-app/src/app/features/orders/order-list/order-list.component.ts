
import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { MatSort } from '@angular/material/sort';
import { OrderService } from '../order.service';
import { GlobalLoadingService } from '../../../shared/global-loading.service';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { MatSnackBar } from '@angular/material/snack-bar';

interface Order {
  id: string;
  customerName: string;
  status: string;
  createdAt: string;
}

@Component({
  selector: 'app-order-list',
  templateUrl: './order-list.component.html',
  styleUrls: ['./order-list.component.css']
})
export class OrderListComponent implements OnInit {
  dataSource = new MatTableDataSource<Order>([]);
  sortActive = 'createdAt';
  sortDirection: 'asc' | 'desc' = 'desc';
  displayedColumns: string[] = ['id', 'customerName', 'status', 'createdAt'];
  totalCount = 0;
  page = 1;
  pageSize = 10;
  loading = false;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  orderService = inject(OrderService);
  snackBar = inject(MatSnackBar);
  globalLoading = inject(GlobalLoadingService);

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading = true;
    this.globalLoading.show();
    this.orderService.listOrders(this.page, this.pageSize, this.sortActive, this.sortDirection === 'desc').subscribe({
      next: (res: { orders: Order[], totalCount: number }) => {
        this.dataSource.data = res.orders;
        this.totalCount = res.totalCount;
        this.loading = false;
        this.globalLoading.hide();
        if (this.paginator) {
          this.dataSource.paginator = this.paginator;
        }
      },
      error: () => {
        this.loading = false;
        this.globalLoading.hide();
      }
    });
  }

  onSort(sort: { active: string, direction: string }): void {
    if (this.sortActive === sort.active) {
      // Toggle direction if same column
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      // New column, default to ascending
      this.sortActive = sort.active;
      this.sortDirection = 'asc';
    }
    this.loadOrders();
  }

  onPage(event: PageEvent): void {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadOrders();
  }
  showCreateOrderModal = false;

  openCreateOrderModal(): void {
    this.showCreateOrderModal = true;
  }

  closeCreateOrderModal(): void {
    this.showCreateOrderModal = false;
  }

  onOrderPlaced(): void {
    this.closeCreateOrderModal();
    this.loadOrders();
    this.snackBar.open('Order placed successfully!', 'Close', { duration: 2000 });
  }

  changeStatus(order: Order, newStatus: string): void {
    if (!newStatus || newStatus === order.status) return;
    this.globalLoading.show();
    this.orderService.updateOrderStatus(order.id, newStatus).subscribe({
      next: () => {
        order.status = newStatus;
        this.globalLoading.hide();
        this.snackBar.open('Order status updated!', 'Close', { duration: 2000 });
      },
      error: () => {
        this.globalLoading.hide();
        this.snackBar.open('Failed to update order status.', 'Close', { duration: 3000 });
      }
    });
  }
}