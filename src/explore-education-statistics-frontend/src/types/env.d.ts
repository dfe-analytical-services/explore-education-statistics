declare namespace NodeJS {
  /**
   * Public env vars needed in the browser are not declared here.
   * Use `getPublicEnv` from `@frontend/utils/publicEnv` for those.
   */
  interface ProcessEnv {
    BUILD_NUMBER: string;
    DEFAULT_CACHE_MAX_AGE_SECONDS: string;
    DEFAULT_CACHE_STALE_WHILE_REVALIDATE_SECONDS?: string;
  }
}
