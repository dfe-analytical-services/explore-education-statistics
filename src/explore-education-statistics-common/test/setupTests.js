import '@testing-library/jest-dom';
import 'core-js/features/array/flat-map';
import 'core-js/features/string/replace-all';
import './setupGlobals';
import './extend-expect';

jest.setTimeout(10000);

if (typeof window !== 'undefined') {
  // fetch polyfill for making API calls.
  require('cross-fetch');
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

require('intersection-observer');
