import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface OrderLine {
  product: string;
  quantity: number;
  price: number;
  currency: string;
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private api = `${environment.apiUrl}/orders`;

  constructor(private http: HttpClient) { }

  placeOrder(customerName: string, lines: OrderLine[]): Observable<{ orderId: string }> {
    return this.http.post<{ orderId: string }>(this.api, { customerName, lines });
  }

  getOrder(id: string): Observable<any> {
    return this.http.get<any>(`${this.api}/${id}`);
  }

  listOrders(page: number = 1, pageSize: number = 10, sortBy?: string, desc: boolean = true): Observable<{ orders: any[], totalCount: number }> {
    let url = `${this.api}?page=${page}&pageSize=${pageSize}`;
    if (sortBy) {
      url += `&sortBy=${sortBy}&desc=${desc}`;
    }
    return this.http.get<{ orders: any[], totalCount: number }>(url);
  }

  updateOrderStatus(id: string, status: string): Observable<void> {
    return this.http.put<void>(`${this.api}/${id}/status`, { status });
  }
}
