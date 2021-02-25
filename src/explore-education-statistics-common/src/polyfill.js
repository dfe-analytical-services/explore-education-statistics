import 'core-js/features/promise';
import 'core-js/features/array/flat';
import 'core-js/features/array/flat-map';
import 'core-js/features/string/replace-all';
import 'cross-fetch/polyfill';
import { enableES5 as immerPolyfill } from 'immer';

immerPolyfill();

if (typeof window !== 'undefined') {
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

  if (!Element.prototype.closest) {
    Element.prototype.closest = function closest(s) {
      // eslint-disable-next-line @typescript-eslint/no-this-alias
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
}
