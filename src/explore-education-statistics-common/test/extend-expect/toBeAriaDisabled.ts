import { matcherHint, printReceived } from 'jest-matcher-utils';

const toBeAriaDisabled: jest.CustomMatcher = function toHaveScrolledIntoView(
  element: HTMLElement,
) {
  if (!(element instanceof window.HTMLElement)) {
    throw new Error('Not a HTMLElement');
  }

  return {
    pass: element.getAttribute('aria-disabled') === 'true',
    message: () => {
      return [
        matcherHint(
          `${this.isNot ? '.not' : ''}.${toBeAriaDisabled.name}`,
          'element',
          '',
        ),
        '',
        `Received element ${
          this.isNot ? 'should not' : 'should'
        } not be aria-disabled:`,
        ` ${printReceived(element.cloneNode(false))}`,
      ].join('\n');
    },
  };
};

export default toBeAriaDisabled;
