if (typeof Promise === 'undefined') {
  window.Promise = require('core-js/es6/promise');
}

export const loadPolyfills = () => {
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

    import('core-js').then(resolve);
  });

  return Promise.all([polyfillCoreJs]);
};
