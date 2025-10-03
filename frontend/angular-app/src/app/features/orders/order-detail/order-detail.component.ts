
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { OrderService } from '../order.service';
import { GlobalLoadingService } from '../../../shared/global-loading.service';

@Component({
  selector: 'app-order-detail',
  templateUrl: './order-detail.component.html',
  styleUrls: ['./order-detail.component.css']
})
export class OrderDetailComponent implements OnInit {
  order: any;
  loading = false;
  constructor(
    private route: ActivatedRoute,
    private orderService: OrderService,
    private router: Router,
    private location: Location,
    private globalLoading: GlobalLoadingService
  ) { }
  back() {
    this.location.back();
  }
  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loading = true;
      this.globalLoading.show();
      this.orderService.getOrder(id).subscribe(o => {
        this.order = o;
        this.loading = false;
        this.globalLoading.hide();
      }, _ => { this.loading = false; this.globalLoading.hide(); });
    }
  }
  updateStatus() {
    const newStatus = this.order._newStatus;
    if (!newStatus || newStatus === this.order.status) return;
    this.globalLoading.show();
    this.orderService.updateOrderStatus(this.order.id, newStatus).subscribe({
      next: () => {
        this.order.status = newStatus;
        this.order._newStatus = undefined;
        this.globalLoading.hide();
        // Optionally show a toast/snackbar here
      },
      error: () => {
        this.globalLoading.hide();
        // Optionally show a toast/snackbar here
      }
    });
  }
}
