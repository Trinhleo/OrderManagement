import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';  // adjust path if needed

@Injectable({ providedIn: 'root' })
export class OrderService {
  private api = `${environment.apiUrl}/orders`;   // ✅ prepend apiUrl

  constructor(private http: HttpClient) { }

  placeOrder(product: string, quantity: number, price: number, currency: string, customerName: string): Observable<{ orderId: string }> {
    return this.http.post<{ orderId: string }>(this.api, { product, quantity, price, currency, customerName });
  }

  getOrder(id: string): Observable<any> {
    return this.http.get<any>(`${this.api}/${id}`);
  }

  listOrders(page: number = 1, pageSize: number = 10): Observable<{ orders: any[], totalCount: number }> {
    return this.http.get<{ orders: any[], totalCount: number }>(`${this.api}?page=${page}&pageSize=${pageSize}`);
  }
}
