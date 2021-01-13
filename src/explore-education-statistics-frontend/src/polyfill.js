import 'cross-fetch/polyfill';
import 'core-js/features/array/flat';
import 'core-js/features/array/flat-map';
import '@common/polyfill';

// Polyfill for dev server breaking in IE11
// See: https://github.com/vercel/next.js/issues/13231
if (typeof window !== 'undefined' && process.env.NODE_ENV === 'development') {
  require('@webcomponents/shadydom');
}
