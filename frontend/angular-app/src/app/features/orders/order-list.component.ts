
import { Component, OnInit, ViewChild } from '@angular/core';
import { OrderService } from './order.service';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';

@Component({
  selector: 'app-order-list',
  template: `
    <mat-toolbar color="primary">
      <span>Orders</span>
      <span class="spacer"></span>
      <button mat-raised-button color="accent" routerLink="/orders/create">Create Order</button>
    </mat-toolbar>
    <div class="mat-elevation-z2" style="margin:24px; padding:24px; background:#fff; border-radius:8px;">
      <mat-progress-spinner *ngIf="loading" mode="indeterminate" diameter="40"></mat-progress-spinner>
      <table mat-table [dataSource]="dataSource" *ngIf="!loading && dataSource.data.length > 0">
        <ng-container matColumnDef="id">
          <th mat-header-cell *matHeaderCellDef>ID</th>
          <td mat-cell *matCellDef="let o"><a [routerLink]="['/orders', o.id]">{{ o.id }}</a></td>
        </ng-container>
        <ng-container matColumnDef="customerName">
          <th mat-header-cell *matHeaderCellDef>Customer</th>
          <td mat-cell *matCellDef="let o">{{ o.customerName }}</td>
        </ng-container>
        <ng-container matColumnDef="status">
          <th mat-header-cell *matHeaderCellDef>Status</th>
          <td mat-cell *matCellDef="let o">{{ o.status }}</td>
        </ng-container>
        <ng-container matColumnDef="createdAt">
          <th mat-header-cell *matHeaderCellDef>Created At</th>
          <td mat-cell *matCellDef="let o">{{ o.createdAt | date:'short' }}</td>
        </ng-container>
        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
      </table>
      <mat-paginator
        [length]="totalCount"
        [pageSize]="pageSize"
        [pageIndex]="page-1"
        [pageSizeOptions]="[5, 10, 20]"
        (page)="onPage($event)"
        *ngIf="!loading && totalCount > pageSize">
      </mat-paginator>
      <div *ngIf="!loading && dataSource.data.length === 0" class="empty-state">
        <mat-icon>inbox</mat-icon>
        <p>No orders found.</p>
      </div>
    </div>
  `,
  styles: [`.spacer { flex: 1 1 auto; } .empty-state { text-align: center; color: #888; margin: 32px 0; } mat-icon { font-size: 48px; }`]
})
export class OrderListComponent implements OnInit {
  dataSource = new MatTableDataSource<any>([]);
  displayedColumns: string[] = ['id', 'customerName', 'status', 'createdAt'];
  totalCount = 0;
  page = 1;
  pageSize = 10;
  loading = false;

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  constructor(private orderService: OrderService) { }

  ngOnInit() {
    this.loadOrders();
  }

  loadOrders() {
    this.loading = true;
    this.orderService.listOrders(this.page, this.pageSize).subscribe(res => {
      this.dataSource.data = res.orders;
      this.totalCount = res.totalCount;
      this.loading = false;
      if (this.paginator) {
        this.dataSource.paginator = this.paginator;
      }
    }, _ => { this.loading = false; });
  }

  onPage(event: any) {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadOrders();
  }
}