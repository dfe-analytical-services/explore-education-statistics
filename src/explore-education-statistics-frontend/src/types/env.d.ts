declare namespace NodeJS {
  interface ProcessEnv {
    APP_ENV: 'Production' | 'Pre-Production' | 'Test' | 'Development';
    APPINSIGHTS_INSTRUMENTATIONKEY: string;
    BUILD_NUMBER: string;
    GA_TRACKING_ID: string;
    NOTIFICATION_API_BASE_URL: string;
    PUBLIC_URL: string;
  }
}
