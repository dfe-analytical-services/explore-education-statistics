import 'core-js/features/promise';
import 'core-js/features/array/flat';
import 'core-js/features/array/flat-map';
import 'core-js/features/string/replace-all';
import 'cross-fetch/polyfill';

if (typeof window !== 'undefined') {
  // NodeList.forEach
  if (window.NodeList && !NodeList.prototype.forEach) {
    NodeList.prototype.forEach = Array.prototype.forEach;
  }

  if (!window.IntersectionObserver) {
    require('intersection-observer');
  }
}
