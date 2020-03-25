import { checkHtmlElement } from '@testing-library/jest-dom/dist/utils';
import {
  matcherHint,
  printReceived,
  RECEIVED_COLOR as receivedColor,
} from 'jest-matcher-utils/build';

const toHaveScrolledIntoView: jest.CustomMatcher = function toHaveScrolledIntoView(
  element: HTMLElement,
) {
  checkHtmlElement(element, toHaveScrolledIntoView, this);

  const scrollIntoViewMock = element.scrollIntoView as jest.Mock;

  if (!jest.isMockFunction(scrollIntoViewMock)) {
    throw new TypeError(
      [
        `${this.isNot ? '.not' : ''}.${toHaveScrolledIntoView.name}`,
        '',
        `${receivedColor('received')} value must be a mock instance`,
      ].join('\n'),
    );
  }

  return {
    pass: scrollIntoViewMock.mock.instances.indexOf(element) > -1,
    message: () => {
      return [
        matcherHint(
          `${this.isNot ? '.not' : ''}.${toHaveScrolledIntoView.name}`,
          'element',
          '',
        ),
        '',
        `Received element ${
          this.isNot ? 'should not' : 'should'
        } have scrolled into view:`,
        ` ${printReceived(element.cloneNode(false))}`,
      ].join('\n');
    },
  };
};

export default toHaveScrolledIntoView;
