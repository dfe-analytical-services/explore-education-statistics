export * from '@common/types';

declare namespace NodeJS {
  interface Process {
    browser: boolean;
  }
}
