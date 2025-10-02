import { ComponentFixture, TestBed } from '@angular/core/testing';
import { OrderListComponent } from './order-list.component';
import { OrderService } from './order.service';
import { of } from 'rxjs';

class MockOrderService {
    listOrders = jasmine.createSpy().and.returnValue(of({ orders: [{ id: '1', customerName: 'A', status: 'New', createdAt: new Date() }], totalCount: 1 }));
}

describe('OrderListComponent', () => {
    let component: OrderListComponent;
    let fixture: ComponentFixture<OrderListComponent>;
    let orderService: MockOrderService;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            declarations: [OrderListComponent],
            providers: [{ provide: OrderService, useClass: MockOrderService }]
        }).compileComponents();

        fixture = TestBed.createComponent(OrderListComponent);
        component = fixture.componentInstance;
        orderService = TestBed.inject(OrderService) as any;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load orders on init', () => {
        expect(orderService.listOrders).toHaveBeenCalled();
        expect(component.dataSource.data.length).toBe(1);
        expect(component.totalCount).toBe(1);
    });
});
