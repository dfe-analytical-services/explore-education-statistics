declare namespace NodeJS {
  interface ProcessEnv {
    APP_ENV: 'Production' | 'Pre-Production' | 'Test' | 'Development';
    APPINSIGHTS_INSTRUMENTATIONKEY: string;
    HOTJAR_ID: string;
    GA_TRACKING_ID: string;
    NOTIFICATION_API_BASE_URL: string;
    PUBLIC_URL: string;
  }
}
