/// <reference types="node" />
/// <reference types="react" />
/// <reference types="react-dom" />

declare namespace NodeJS {
  interface ProcessEnv {
    APP_ROOT_ID: string;
    NODE_ENV: 'development' | 'production' | 'test';
    PUBLIC_URL?: string;
    CONTENT_API_BASE_URL?: string;
    DATA_API_BASE_URL?: string;
  }
}
