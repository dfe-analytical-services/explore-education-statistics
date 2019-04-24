if (typeof Promise === 'undefined') {
  // eslint-disable-next-line global-require
  window.Promise = require('core-js/fn/promise');
}

const loadPolyfill = () => {
  const polyfillCoreJs = new Promise(resolve => {
    if (
      'startsWith' in String.prototype &&
      'endsWith' in String.prototype &&
      'includes' in Array.prototype &&
      'assign' in Object &&
      'keys' in Object
    ) {
      return resolve();
    }

    return import('core-js').then(resolve);
  });

  const applyCustomPolyfills = () => {
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
  };

  return polyfillCoreJs.then(applyCustomPolyfills);
};

export default loadPolyfill;
