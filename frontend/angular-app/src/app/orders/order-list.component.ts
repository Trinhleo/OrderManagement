import { Component, OnInit } from '@angular/core';
import { OrderService } from './order.service';

@Component({
  selector: 'app-order-list',
  template: '<div *ngFor="let o of orders">{{ o.id }} - {{ o.createdAt }}</div>'
})
export class OrderListComponent implements OnInit {
  orders: any[] = [];
  constructor(private orderService: OrderService) {}

  ngOnInit() {
    // Demo only - would need API to list orders
  }
}