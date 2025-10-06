import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface OrderLine {
  product: string;
  quantity: number;
  price: number;
  currency: string;
}

export interface Order {
  id: string;
  customerName: string;
  status: string;
  createdAt: string;
  lines: OrderLine[];
  _newStatus?: string;
}

export interface PlaceOrderCommand {
  customerName: string;
  lines: OrderLine[];
}

export interface PlaceOrderResponse {
  orderId: string;
}

export interface ListOrdersResponse {
  orders: Order[];
  totalCount: number;
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private api = `${environment.apiUrl}/orders`;
  private http = inject(HttpClient);

  placeOrder(command: PlaceOrderCommand): Observable<PlaceOrderResponse> {
    return this.http.post<PlaceOrderResponse>(this.api, command);
  }

  // Convenience method for backward compatibility
  placeOrderSimple(customerName: string, lines: OrderLine[]): Observable<PlaceOrderResponse> {
    return this.placeOrder({ customerName, lines });
  }

  getOrder(id: string): Observable<Order> {
    return this.http.get<Order>(`${this.api}/${id}`);
  }

  listOrders(page = 1, pageSize = 10, sortBy?: string, desc = true): Observable<ListOrdersResponse> {
    let url = `${this.api}?page=${page}&pageSize=${pageSize}`;
    if (sortBy) {
      url += `&sortBy=${sortBy}&desc=${desc}`;
    }
    return this.http.get<ListOrdersResponse>(url);
  }

  updateOrderStatus(id: string, status: string): Observable<void> {
    return this.http.put<void>(`${this.api}/${id}/status`, { status });
  }
}
