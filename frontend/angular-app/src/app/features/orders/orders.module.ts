import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { OrdersRoutingModule } from './orders-routing.module';
import { OrderListComponent } from './order-list.component';
import { OrderCreateComponent } from './order-create.component';
import { OrderDetailComponent } from './order-detail.component';
import { OrderService } from './order.service';

@NgModule({
    declarations: [OrderListComponent, OrderCreateComponent, OrderDetailComponent],
    imports: [
        CommonModule,
        FormsModule,
        OrdersRoutingModule,
        MatToolbarModule,
        MatButtonModule,
        MatTableModule,
        MatPaginatorModule,
        MatProgressSpinnerModule,
        MatSnackBarModule,
        MatInputModule,
        MatIconModule
    ],
    providers: [OrderService]
})
export class OrdersModule { }
