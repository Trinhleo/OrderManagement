import { NgModule } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { LocalDatePipe } from './local-date.pipe';

@NgModule({
    declarations: [
        LocalDatePipe
    ],
    imports: [
        CommonModule
    ],
    providers: [
        DatePipe
    ],
    exports: [
        LocalDatePipe
    ]
})
export class SharedModule { }