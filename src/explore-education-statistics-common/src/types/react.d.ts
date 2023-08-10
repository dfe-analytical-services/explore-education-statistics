declare module 'react' {
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  interface HTMLAttributes<T> {
    inert?: '' | undefined;
  }
}

declare global {
  namespace JSX {
    interface IntrinsicAttributes {
      inert?: '' | undefined;
    }
  }
}

export {};
