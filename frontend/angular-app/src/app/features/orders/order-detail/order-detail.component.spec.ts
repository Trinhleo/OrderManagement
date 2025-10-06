import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { of } from 'rxjs';
import { OrderDetailComponent } from './order-detail.component';
import { OrderService } from '../order.service';
import { GlobalLoadingService } from '../../../shared/global-loading.service';

class MockOrderService {
    getOrder = jasmine.createSpy().and.returnValue(of({
        id: '1',
        customerName: 'A',
        status: 'New',
        createdAt: '2025-10-06T10:00:00Z',
        lines: []
    }));
    updateOrderStatus = jasmine.createSpy().and.returnValue(of(void 0));
}

class MockGlobalLoadingService {
    show = jasmine.createSpy();
    hide = jasmine.createSpy();
}

class MockLocation {
    back = jasmine.createSpy();
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
                { provide: ActivatedRoute, useValue: routeStub },
                { provide: GlobalLoadingService, useClass: MockGlobalLoadingService },
                { provide: Location, useClass: MockLocation }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(OrderDetailComponent);
        component = fixture.componentInstance;
        orderService = TestBed.inject(OrderService) as jasmine.SpyObj<OrderService>;
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
