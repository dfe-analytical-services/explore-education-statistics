declare namespace NodeJS {
  interface ProcessEnv {
    APP_ROOT_ID: string;
    NODE_ENV: 'development' | 'production' | 'test';
  }
}
