import { ComponentFixture, TestBed } from '@angular/core/testing';
import { OrderListComponent } from './order-list.component';
import { OrderService } from '../order.service';
import { GlobalLoadingService } from '../../../shared/global-loading.service';
import { ToastService } from '../../../shared/toast.service';
import { of } from 'rxjs';

class MockOrderService {
    listOrders = jasmine.createSpy().and.returnValue(of({
        orders: [{
            id: '1',
            customerName: 'A',
            status: 'New',
            createdAt: '2025-10-06T10:00:00Z',
            lines: []
        }],
        totalCount: 1
    }));
    updateOrderStatus = jasmine.createSpy().and.returnValue(of(void 0));
}

class MockGlobalLoadingService {
    show = jasmine.createSpy();
    hide = jasmine.createSpy();
}

class MockToastService {
    success = jasmine.createSpy();
    error = jasmine.createSpy();
    warning = jasmine.createSpy();
    info = jasmine.createSpy();
}

describe('OrderListComponent', () => {
    let component: OrderListComponent;
    let fixture: ComponentFixture<OrderListComponent>;
    let orderService: MockOrderService;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            declarations: [OrderListComponent],
            providers: [
                { provide: OrderService, useClass: MockOrderService },
                { provide: GlobalLoadingService, useClass: MockGlobalLoadingService },
                { provide: ToastService, useClass: MockToastService }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(OrderListComponent);
        component = fixture.componentInstance;
        orderService = TestBed.inject(OrderService) as jasmine.SpyObj<OrderService>;
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
