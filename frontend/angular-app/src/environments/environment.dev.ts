export const environment = {
    production: false,
    apiUrl: 'https://localhost:56642/api',
    // Alternative endpoints for testing
    apiUrlHttp: 'http://localhost:56643/api', // HTTP fallback
    apiUrlDirect: 'https://localhost:56642/api', // Direct HTTPS
    // SSL configuration
    allowUntrustedCertificates: true
};