/// <reference types="jest-dom/extend-expect" />

declare module 'jest-dom/dist/utils' {
  export const checkHasWindow: (element: unknown) => void;
  export const checkHtmlElement: (
    element: unknown,
    matcherFn?: Function,
    context?: jest.MatcherUtils,
  ) => void;
}

declare namespace jest {
  interface Matchers<R> {
    toHaveScrolledIntoView(): R;
  }
}
