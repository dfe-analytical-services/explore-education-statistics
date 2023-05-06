declare namespace NodeJS {
  interface ProcessEnv {
    APP_ENV: 'Production' | 'Pre-Production' | 'Test' | 'Development';
    APPINSIGHTS_INSTRUMENTATIONKEY: string;
    NEXT_PUBLIC_BUILD_NUMBER: string;
    NEXT_PUBLIC_GA_TRACKING_ID: string;
    NEXT_PUBLIC_NOTIFICATION_API_BASE_URL: string;
    NEXT_PUBLIC_CONTENT_API_BASE_URL: string;
    NEXT_PUBLIC_PUBLIC_URL: string;
    BASIC_AUTH: boolean;
    BASIC_AUTH_USER: string;
    BASIC_AUTH_PASSWORD: string;
  }
}
