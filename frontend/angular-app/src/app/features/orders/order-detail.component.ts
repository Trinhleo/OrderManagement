
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { OrderService } from './order.service';

@Component({
  selector: 'app-order-detail',
  template: `
    <mat-toolbar color="primary">
      <span>Order Details</span>
    </mat-toolbar>
    <div class="mat-elevation-z2" style="margin:24px; padding:24px; background:#fff; border-radius:8px; max-width:600px;">
      <mat-progress-spinner *ngIf="loading" mode="indeterminate" diameter="40"></mat-progress-spinner>
      <mat-card *ngIf="order">
        <mat-card-title>Order #{{ order.id }}</mat-card-title>
        <mat-card-content>
          <div><b>Customer:</b> {{ order.customerName }}</div>
          <div><b>Status:</b> {{ order.status }}</div>
          <div><b>Created At:</b> {{ order.createdAt | date:'short' }}</div>
          <h3>Lines</h3>
          <table mat-table [dataSource]="order.lines" *ngIf="order.lines && order.lines.length > 0">
            <ng-container matColumnDef="product">
              <th mat-header-cell *matHeaderCellDef>Product</th>
              <td mat-cell *matCellDef="let line">{{ line.product }}</td>
            </ng-container>
            <ng-container matColumnDef="quantity">
              <th mat-header-cell *matHeaderCellDef>Quantity</th>
              <td mat-cell *matCellDef="let line">{{ line.quantity }}</td>
            </ng-container>
            <ng-container matColumnDef="price">
              <th mat-header-cell *matHeaderCellDef>Price</th>
              <td mat-cell *matCellDef="let line">{{ line.amount }} {{ line.currency }}</td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="['product','quantity','price']"></tr>
            <tr mat-row *matRowDef="let row; columns: ['product','quantity','price'];"></tr>
          </table>
          <div *ngIf="!order.lines || order.lines.length === 0" class="empty-state">
            <mat-icon>inbox</mat-icon> No lines
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`.empty-state { text-align: center; color: #888; margin: 32px 0; } mat-icon { font-size: 32px; }`]
})
export class OrderDetailComponent implements OnInit {
  order: any;
  loading = false;
  constructor(private route: ActivatedRoute, private orderService: OrderService) { }
  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loading = true;
      this.orderService.getOrder(id).subscribe(o => {
        this.order = o;
        this.loading = false;
      }, _ => { this.loading = false; });
    }
  }
}
