import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';

export const debugInterceptor: HttpInterceptorFn = (req, next) => {
    console.log('🚀 HTTP Request:', {
        method: req.method,
        url: req.url,
        headers: req.headers.keys().reduce((acc, key) => ({
            ...acc,
            [key]: req.headers.get(key)
        }), {}),
        body: req.body
    });

    const startTime = Date.now();

    return next(req).pipe(
        catchError((error: HttpErrorResponse) => {
            const duration = Date.now() - startTime;

            console.error('❌ HTTP Error:', {
                status: error.status,
                statusText: error.statusText,
                url: error.url,
                message: error.message,
                error: error.error,
                duration: `${duration}ms`,
                headers: error.headers?.keys()?.reduce((acc, key) => ({
                    ...acc,
                    [key]: error.headers.get(key)
                }), {})
            });

            // Handle specific errors
            if (error.status === 0) {
                console.error('🚨 Network Error - Possible causes:');
                console.error('- CORS not configured properly');
                console.error('- SSL certificate issues');
                console.error('- API server not running');
                console.error('- Firewall blocking the request');
            }

            return throwError(() => error);
        })
    );
};