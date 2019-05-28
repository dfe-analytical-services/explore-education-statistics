declare module 'react-hotjar' {
  interface Hotjar {
    initialize(key: string, version: number): void;
  }

  // eslint-disable-next-line import/prefer-default-export
  export const hotjar: Hotjar;
}
