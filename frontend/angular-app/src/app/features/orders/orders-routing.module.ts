import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { OrderListComponent } from './order-list.component';
import { OrderCreateComponent } from './order-create.component';
import { OrderDetailComponent } from './order-detail.component';

const routes: Routes = [
    { path: '', component: OrderListComponent },
    { path: 'create', component: OrderCreateComponent },
    { path: ':id', component: OrderDetailComponent }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class OrdersRoutingModule { }
