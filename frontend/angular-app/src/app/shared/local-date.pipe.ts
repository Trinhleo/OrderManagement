import { Pipe, PipeTransform } from '@angular/core';
import { DatePipe } from '@angular/common';

@Pipe({
    name: 'localDate',
    standalone: false
})
export class LocalDatePipe implements PipeTransform {

    constructor(private datePipe: DatePipe) { }

    transform(value: any, format: string = 'medium', timezone?: string, locale?: string): string | null {
        if (!value) {
            return null;
        }

        // Convert UTC time to local time
        let date: Date;

        if (typeof value === 'string') {
            // If the string doesn't end with 'Z', assume it's UTC and add 'Z'
            if (!value.endsWith('Z') && !value.includes('+') && !value.includes('-', 10)) {
                date = new Date(value + 'Z');
            } else {
                date = new Date(value);
            }
        } else {
            date = new Date(value);
        }

        // Use Angular's DatePipe with local timezone
        return this.datePipe.transform(date, format, undefined, locale);
    }
}