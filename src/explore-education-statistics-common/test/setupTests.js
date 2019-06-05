import 'react-testing-library/cleanup-after-each';
import 'jest-dom/extend-expect';
import 'core-js/fn/array/virtual/flat-map';
import './setupGlobals';

if (typeof window !== 'undefined') {
  // fetch polyfill for making API calls.
  require('cross-fetch');
}

// fix for jsdom not working with SVGs
const createElementNSOrig = global.document.createElementNS;
// eslint-disable-next-line
global.document.createElementNS = function(namespaceURI, qualifiedName) {
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
