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

@Injectable({ providedIn: 'root' })
export class OrderService {
  private api = `${environment.apiUrl}/orders`;
  private http = inject(HttpClient);

  placeOrder(customerName: string, lines: OrderLine[]): Observable<{ orderId: string }> {
    return this.http.post<{ orderId: string }>(this.api, { customerName, lines });
  }

  getOrder(id: string): Observable<Order> {
    return this.http.get<Order>(`${this.api}/${id}`);
  }

  listOrders(page = 1, pageSize = 10, sortBy?: string, desc = true): Observable<{ orders: Order[], totalCount: number }> {
    let url = `${this.api}?page=${page}&pageSize=${pageSize}`;
    if (sortBy) {
      url += `&sortBy=${sortBy}&desc=${desc}`;
    }
    return this.http.get<{ orders: Order[], totalCount: number }>(url);
  }

  updateOrderStatus(id: string, status: string): Observable<void> {
    return this.http.put<void>(`${this.api}/${id}/status`, { status });
  }
}
