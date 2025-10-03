
import { Component, OnInit, ViewChild } from '@angular/core';
import { MatSort } from '@angular/material/sort';
import { OrderService } from '../order.service';
import { GlobalLoadingService } from '../../../shared/global-loading.service';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-order-list',
  templateUrl: './order-list.component.html',
  styleUrls: ['./order-list.component.css']
})
export class OrderListComponent implements OnInit {
  dataSource = new MatTableDataSource<any>([]);
  sortActive = 'createdAt';
  sortDirection: 'asc' | 'desc' = 'desc';
  displayedColumns: string[] = ['id', 'customerName', 'status', 'createdAt'];
  totalCount = 0;
  page = 1;
  pageSize = 10;
  loading = false;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(private orderService: OrderService, private snackBar: MatSnackBar, private globalLoading: GlobalLoadingService) { }

  ngOnInit() {
    this.loadOrders();
  }

  loadOrders() {
    this.loading = true;
    this.globalLoading.show();
    this.orderService.listOrders(this.page, this.pageSize, this.sortActive, this.sortDirection === 'desc').subscribe(res => {
      this.dataSource.data = res.orders;
      this.totalCount = res.totalCount;
      this.loading = false;
      this.globalLoading.hide();
      if (this.paginator) {
        this.dataSource.paginator = this.paginator;
      }
    }, _ => { this.loading = false; this.globalLoading.hide(); });
  }

  onSort(sort: { active: string, direction: string }) {
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

  onPage(event: any) {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadOrders();
  }
  showCreateOrderModal = false;

  openCreateOrderModal() {
    this.showCreateOrderModal = true;
  }

  closeCreateOrderModal() {
    this.showCreateOrderModal = false;
  }

  onOrderPlaced() {
    this.closeCreateOrderModal();
    this.loadOrders();
    this.snackBar.open('Order placed successfully!', 'Close', { duration: 2000 });
  }

  changeStatus(order: any, newStatus: string) {
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
        this.snackBar.open('Failed to update status.', 'Close', { duration: 3000 });
      }
    });
  }
}