import { Component } from '@angular/core';
import { OrderService } from './order.service';

@Component({
  selector: 'app-order-create',
  template: '<button (click)="place()">Place Order</button>'
})
export class OrderCreateComponent {
  constructor(private orderService: OrderService) {}

  place() {
    this.orderService.placeOrder('Laptop', 1, 1000, 'USD').subscribe(res => {
      console.log('Order placed:', res);
    });
  }
}