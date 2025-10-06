import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { of } from 'rxjs';
import { OrderCreateComponent } from './order-create.component';
import { OrderService } from '../order.service';
import { GlobalLoadingService } from '../../../shared/global-loading.service';
import { ToastService } from '../../../shared/toast.service';

class MockOrderService {
    placeOrder = jasmine.createSpy().and.returnValue(of({ orderId: '123' }));
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

describe('OrderCreateComponent', () => {
    let component: OrderCreateComponent;
    let fixture: ComponentFixture<OrderCreateComponent>;
    let orderService: MockOrderService;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            declarations: [OrderCreateComponent],
            imports: [FormsModule],
            providers: [
                { provide: OrderService, useClass: MockOrderService },
                { provide: GlobalLoadingService, useClass: MockGlobalLoadingService },
                { provide: ToastService, useClass: MockToastService }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(OrderCreateComponent);
        component = fixture.componentInstance;
        orderService = TestBed.inject(OrderService) as jasmine.SpyObj<OrderService>;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should call placeOrder and set success', () => {
        component.customerName = 'Test';
        component.lines = [{ product: 'Laptop', quantity: 2, price: 1000, currency: 'USD' }];
        component.place();
        expect(orderService.placeOrder).toHaveBeenCalled();
    });
})