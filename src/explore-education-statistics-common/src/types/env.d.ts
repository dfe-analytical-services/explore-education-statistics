declare namespace NodeJS {
  interface ProcessEnv {
    APP_ROOT_ID: string;
    CONTENT_API_BASE_URL: string;
    DATA_API_BASE_URL: string;
    NODE_ENV: 'development' | 'production' | 'test';
    PUBLIC_API_BASE_URL: string;
    PUBLIC_URL: string;
  }
}
