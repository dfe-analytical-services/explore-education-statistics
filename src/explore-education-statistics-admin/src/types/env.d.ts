interface ImportMetaEnv {
  VITE_BUILD_NUMBER: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}

declare namespace NodeJS {
  // eslint-disable-next-line @typescript-eslint/no-empty-interface
  interface ProcessEnv extends ImportMetaEnv {}
}
