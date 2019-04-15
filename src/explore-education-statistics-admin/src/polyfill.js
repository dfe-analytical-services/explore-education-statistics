console.log(typeof Promise === 'undefined');

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
  };

  return polyfillCoreJs.then(applyCustomPolyfills);
};

export default loadPolyfill;
