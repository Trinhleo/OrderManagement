import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-api-test',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div style="padding: 20px; border: 1px solid #ccc; margin: 10px; background: #f9f9f9;">
      
      <div *ngIf="results().length > 0" style="margin-top: 20px;">
        <h4>Results:</h4>
        <div *ngFor="let result of results()" 
             [style.color]="result.success ? 'green' : 'red'"
             style="font-family: monospace; margin: 5px 0;">
          {{ result.timestamp }} - {{ result.test }}: {{ result.message }}
        </div>
      </div>
    </div>
  `
})
export class ApiTestComponent {
    private readonly http = inject(HttpClient);

    readonly testing = signal(false);
    readonly results = signal<Array<{
        test: string;
        success: boolean;
        message: string;
        timestamp: string;
    }>>([]);

    testHealth(): void {
        this.testing.set(true);

        const healthUrls = [
            'https://localhost:56642/health',
            'http://localhost:56643/health'
        ];

        healthUrls.forEach(url => {
            this.http.get(url, { responseType: 'text' }).subscribe({
                next: (response) => {
                    this.addResult('Health', true, `✅ ${url} - Response: ${response}`);
                    this.testing.set(false);
                },
                error: (error) => {
                    this.addResult('Health', false, `❌ ${url} - Error: ${error.status} ${error.message}`);
                    this.testing.set(false);
                }
            });
        });
    }

    testLogin(): void {
        this.testing.set(true);

        const loginUrls = [
            'https://localhost:56642/api/Auth/login',
            'http://localhost:56643/api/Auth/login'
        ];

        const credentials = { username: 'admin', password: 'admin123' };

        loginUrls.forEach(url => {
            this.http.post(url, credentials).subscribe({
                next: (response) => {
                    this.addResult('Login', true, `✅ ${url} - Success: ${JSON.stringify(response)}`);
                    this.testing.set(false);
                },
                error: (error) => {
                    this.addResult('Login', false, `❌ ${url} - Error: ${error.status} ${error.message}`);
                    this.testing.set(false);
                }
            });
        });
    }

    testCors(): void {
        this.testing.set(true);

        // Test CORS with a simple OPTIONS request
        const url = 'https://localhost:56642/api/Auth/login';

        this.http.request('OPTIONS', url).subscribe({
            next: (response) => {
                this.addResult('CORS', true, `✅ CORS preflight successful`);
                this.testing.set(false);
            },
            error: (error) => {
                this.addResult('CORS', false, `❌ CORS preflight failed: ${error.status} ${error.message}`);
                this.testing.set(false);
            }
        });
    }

    private addResult(test: string, success: boolean, message: string): void {
        const timestamp = new Date().toLocaleTimeString();
        this.results.update(results => [
            ...results,
            { test, success, message, timestamp }
        ]);
    }
}