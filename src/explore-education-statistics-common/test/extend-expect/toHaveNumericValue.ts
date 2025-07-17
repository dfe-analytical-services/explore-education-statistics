import { matcherHint } from 'jest-matcher-utils';

const toHaveNumericValue: jest.CustomMatcher = function toHaveNumericValue(
  element: HTMLInputElement,
  numberValue: string | number,
) {
  if (!(element instanceof window.HTMLElement)) {
    throw new Error('Not a HTMLElement');
  }

  if (Number.isNaN(numberValue)) {
    throw new Error('numberValue isNaN');
  }

  const elementValue = element.value;

  return {
    pass: String(numberValue) === String(elementValue),
    message: () => {
      return [
        matcherHint(
          `${this.isNot ? '.not' : ''}.${toHaveNumericValue.name}`,
          'element',
          '',
        ),

        element.outerHTML,

        '',
        `Received element ${
          this.isNot ? 'should not' : 'should'
        } have numeric value:`,
        `Expected ${numberValue} (${typeof numberValue}), Received: ${elementValue} (${typeof elementValue})`,
      ].join('\n');
    },
  };
};

export default toHaveNumericValue;
