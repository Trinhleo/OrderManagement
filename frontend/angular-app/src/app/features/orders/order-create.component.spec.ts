import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { of } from 'rxjs';
import { OrderCreateComponent } from './order-create.component';
import { OrderService } from './order.service';

class MockOrderService {
    placeOrder = jasmine.createSpy().and.returnValue(of({ orderId: '123' }));
}

describe('OrderCreateComponent', () => {
    let component: OrderCreateComponent;
    let fixture: ComponentFixture<OrderCreateComponent>;
    let orderService: MockOrderService;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            declarations: [OrderCreateComponent],
            imports: [FormsModule],
            providers: [{ provide: OrderService, useClass: MockOrderService }]
        }).compileComponents();

        fixture = TestBed.createComponent(OrderCreateComponent);
        component = fixture.componentInstance;
        orderService = TestBed.inject(OrderService) as any;
        fixture.detectChanges();
    });
    it('should create', () => {
        expect(component).toBeTruthy();
    });
    it('should call placeOrder and set success', () => {
        component.customerName = 'Test';
        component.lines = [{ product: 'Laptop', quantity: 2, price: 1000, currency: 'USD' }];
        component.place();
        expect(orderService.placeOrder).toHaveBeenCalledWith('Laptop', 2, 1000, 'USD', 'Test');
        expect(component.success).toBeTrue();
        expect(component.orderId).toBe('123');
    });
})