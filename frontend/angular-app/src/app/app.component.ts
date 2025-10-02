import { Component } from '@angular/core';

@Component({
    selector: 'app-root',
    template: `
    <nav>
      <a routerLink="/orders" routerLinkActive="active">Order List</a> |
      <a routerLink="/orders/create" routerLinkActive="active">Create Order</a>
    </nav>
    <router-outlet></router-outlet>
  `,
    styles: [`nav { margin-bottom: 20px; } a.active { font-weight: bold; }`]
})
export class AppComponent { }
