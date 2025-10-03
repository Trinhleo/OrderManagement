import { Component, inject } from '@angular/core';
import { GlobalLoadingService } from './global-loading.service';

@Component({
    selector: 'app-global-spinner',
    templateUrl: './global-spinner.component.html',
})
export class GlobalSpinnerComponent {
    globalLoading$ = inject(GlobalLoadingService).loading$;
}
