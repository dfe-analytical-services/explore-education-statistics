declare module '@testing-library/jest-dom/dist/utils' {
  // eslint-disable-next-line import/prefer-default-export
  export const checkHtmlElement: (
    element: unknown,
    // eslint-disable-next-line @typescript-eslint/ban-types
    matcherFn?: Function,
    context?: jest.MatcherUtils,
  ) => void;
}

declare namespace jest {
  interface Matchers<R> {
    toBeAriaDisabled(): R;
    toHaveScrolledIntoView(): R;
  }
}
