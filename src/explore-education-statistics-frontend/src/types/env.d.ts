declare namespace NodeJS {
  interface ProcessEnv {
    APP_ENV: 'Production' | 'Pre-Production' | 'Test' | 'Development' | 'Local';
    APPINSIGHTS_INSTRUMENTATIONKEY: string;
    BUILD_NUMBER: string;
    CONTENT_API_BASE_URL: string;
    DATA_API_BASE_URL: string;
    GA_TRACKING_ID: string;
    NOTIFICATION_API_BASE_URL: string;
    PUBLIC_API_DOCS_URL: string;
    PUBLIC_API_BASE_URL: string;
    PUBLIC_URL: string;
  }
}
