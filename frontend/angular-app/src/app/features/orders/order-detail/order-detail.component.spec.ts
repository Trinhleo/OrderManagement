import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { OrderDetailComponent } from './order-detail.component';
import { OrderService } from '../order.service';

class MockOrderService {
    getOrder = jasmine.createSpy().and.returnValue(of({ id: '1', customerName: 'A', status: 'New', createdAt: new Date(), lines: [] }));
}

describe('OrderDetailComponent', () => {
    let component: OrderDetailComponent;
    let fixture: ComponentFixture<OrderDetailComponent>;
    let orderService: MockOrderService;
    let routeStub: { snapshot: { paramMap: { get: () => string } } };

    beforeEach(async () => {
        routeStub = { snapshot: { paramMap: { get: () => '1' } } };
        await TestBed.configureTestingModule({
            declarations: [OrderDetailComponent],
            providers: [
                { provide: OrderService, useClass: MockOrderService },
                { provide: ActivatedRoute, useValue: routeStub }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(OrderDetailComponent);
        component = fixture.componentInstance;
        orderService = TestBed.inject(OrderService) as MockOrderService;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load order on init', () => {
        expect(orderService.getOrder).toHaveBeenCalledWith('1');
        expect(component.order).toBeTruthy();
        expect(component.order?.id).toBe('1');
    });
});
