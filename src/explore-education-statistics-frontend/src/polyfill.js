import 'cross-fetch/polyfill';
import 'core-js/features/array/flat';
import 'core-js/features/array/flat-map';
import '@common/polyfill';
import { enableES5 as immerPolyfill } from 'immer';

// Immer polyfill needs to be applied in
// both the frontend and common.
immerPolyfill();

// Polyfill for dev server breaking in IE11
// See: https://github.com/vercel/next.js/issues/13231
if (typeof window !== 'undefined' && process.env.NODE_ENV === 'development') {
  require('@webcomponents/shadydom');
}
