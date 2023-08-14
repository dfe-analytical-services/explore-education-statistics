declare module 'react-router' {
  export * from 'react-router/index';

  export function generatePath<
    Params extends {
      [paramName: string]: string | number | boolean | undefined;
    },
  >(pattern: string, params: Params): string;
}
