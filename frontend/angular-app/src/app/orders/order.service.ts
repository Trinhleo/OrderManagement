import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private api = '/api/orders';
  constructor(private http: HttpClient) {}

  placeOrder(product: string, quantity: number, price: number, currency: string): Observable<any> {
    return this.http.post<{ orderId: string }>(this.api, { product, quantity, price, currency });
  }

  getOrder(id: string): Observable<any> {
    return this.http.get<any>(`${this.api}/${id}`);
  }
}