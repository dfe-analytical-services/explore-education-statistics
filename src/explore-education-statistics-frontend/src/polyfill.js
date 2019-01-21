if (typeof Promise === 'undefined') {
  require('promise/lib/rejection-tracking').enable();
  window.Promise = require('promise/lib/es6-extensions.js');
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
