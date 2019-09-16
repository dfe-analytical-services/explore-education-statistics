import 'cross-fetch/polyfill';

if (typeof Promise === 'undefined') {
  window.Promise = require('core-js/fn/promise');
}

// Load entirety of CoreJS if using a browser
// that doesn't support core features (IE11).
// This probably shouldn't be needed as long
// as babel-preset-env is working correctly.
if (
  !String.prototype.startsWith ||
  !String.prototype.endsWith ||
  !Array.prototype.includes ||
  !Object.assign ||
  !Object.keys
) {
  require('core-js');
} else {
  require('core-js/fn/array/virtual/flatten');
  require('core-js/fn/array/virtual/flat-map');
}

// Alias Array.flat to Array.flatten as
// core-js@2 uses the older proposal
if (!Array.prototype.flat) {
  // eslint-disable-next-line no-extend-native
  Array.prototype.flat = Array.prototype.flatten;
}

// NodeList.forEach
if (window.NodeList && !NodeList.prototype.forEach) {
  NodeList.prototype.forEach = Array.prototype.forEach;
}

// For IE11

if (!Element.prototype.matches) {
  Element.prototype.matches =
    Element.prototype.msMatchesSelector ||
    Element.prototype.webkitMatchesSelector;
}

/* eslint-disable */
(function() {
  if (typeof window.CustomEvent === 'function') return false;

  function CustomEvent(event, params) {
    params = params || { bubbles: false, cancelable: false, detail: null };
    var evt = document.createEvent('CustomEvent');
    evt.initCustomEvent(
      event,
      params.bubbles,
      params.cancelable,
      params.detail,
    );
    return evt;
  }

  Event = CustomEvent;
})();
/* eslint-enable */

if (!Element.prototype.closest) {
  Element.prototype.closest = function closest(s) {
    let el = this;

    do {
      if (el.matches(s)) return el;
      el = el.parentElement || el.parentNode;
    } while (el !== null && el !== undefined && el.nodeType === 1);
    return null;
  };
}

['classList'].forEach(propertyName => {
  if (
    propertyName in HTMLElement.prototype &&
    !(propertyName in SVGElement.prototype)
  ) {
    const desc = Object.getOwnPropertyDescriptor(
      HTMLElement.prototype,
      propertyName,
    );
    Object.defineProperty(SVGElement.prototype, propertyName, desc);
  }
});

if (!window.IntersectionObserver) {
  require('intersection-observer');
}
