declare module '@testing-library/jest-dom/dist/utils' {
  // eslint-disable-next-line import/prefer-default-export
  export const checkHtmlElement: (
    element: unknown,
    // eslint-disable-next-line @typescript-eslint/no-unsafe-function-type
    matcherFn?: Function,
    context?: jest.MatcherUtils,
  ) => void;
}

declare namespace jest {
  interface Matchers<R> {
    toBeAriaDisabled(): R;
    toHaveScrolledIntoView(): R;
    toHaveNumericValue(numberValue: string | number): R;
  }
}
