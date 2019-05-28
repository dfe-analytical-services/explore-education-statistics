import 'cross-fetch/polyfill';
import 'core-js/fn/array/virtual/flatten';
import 'core-js/fn/array/virtual/flat-map';

if (typeof Promise === 'undefined') {
  window.Promise = require('core-js/fn/promise');
}

// Load entirety of CoreJS if using a browser
// that doesn't support core features (IE11).
// This probably shouldn't be needed as long
// as babel-preset-env is working correctly.
if (
  !('startsWith' in String.prototype) ||
  !('endsWith' in String.prototype) ||
  !('includes' in Array.prototype) ||
  !('assign' in Object) ||
  !('keys' in Object)
) {
  require('core-js');
}

if (!('repeat' in String.prototype)) {
  require('core-js/fn/string/repeat');
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

// Alias addListener/removeListener (as these are deprecated)
if (
  typeof MediaQueryList.prototype.addEventListener === 'undefined' ||
  typeof MediaQueryList.prototype.removeEventListener === 'undefined'
) {
  MediaQueryList.prototype.addEventListener =
    MediaQueryList.prototype.addListener;
  MediaQueryList.prototype.removeEventListener =
    MediaQueryList.prototype.removeListener;
}

// For IE11

if (!Element.prototype.matches) {
  Element.prototype.matches =
    Element.prototype.msMatchesSelector ||
    Element.prototype.webkitMatchesSelector;
}

if (!Element.prototype.closest) {
  Element.prototype.closest = s => {
    let el = this;

    do {
      if (el.matches(s)) return el;
      el = el.parentElement || el.parentNode;
    } while (el !== null && el.nodeType === 1);
    return null;
  };
}
