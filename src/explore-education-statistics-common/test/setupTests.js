import errorOnConsoleError from '@common-test/errorOnConsoleError';
import '@testing-library/jest-dom';
import './extend-expect';
import './setupGlobals';

jest.setTimeout(10000);

if (typeof window !== 'undefined') {
  require('intersection-observer');
}

// fix for jsdom not working with SVGs
const createElementNSOrig = global.document.createElementNS;
// eslint-disable-next-line
global.document.createElementNS = function (namespaceURI, qualifiedName) {
  if (
    namespaceURI === 'http://www.w3.org/2000/svg' &&
    qualifiedName === 'svg'
  ) {
    // eslint-disable-next-line prefer-rest-params
    const element = createElementNSOrig.apply(this, arguments);
    element.createSVGRect = () => {};
    return element;
  }
  // eslint-disable-next-line prefer-rest-params
  return createElementNSOrig.apply(this, arguments);
};

errorOnConsoleError();
