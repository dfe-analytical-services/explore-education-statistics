declare namespace NodeJS {
  interface ProcessEnv {
    APP_ENV: 'Production' | 'Pre-Production' | 'Test' | 'Development';
    APPINSIGHTS_INSTRUMENTATIONKEY: string;
    BUILD_NUMBER: string;
    NEXT_PUBLIC_GA_TRACKING_ID: string;
    NEXT_PUBLIC_NOTIFICATION_API_BASE_URL: string;
    NEXT_PUBLIC_URL: string;
  }
}
