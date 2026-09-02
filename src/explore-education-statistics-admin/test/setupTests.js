import errorOnConsoleError from '@common-test/errorOnConsoleError';
import '@common-test/extend-expect';
import '@common-test/setupGlobals';
import '@testing-library/jest-dom';

jest.setTimeout(10000);

if (typeof window !== 'undefined') {
  require('intersection-observer');
}

/**
 * This is a bit of a hack to fix problems with using creating data routers, as some of the
 * required global objects in the browser aren't available.
 */
if (typeof global.Request === 'undefined') {
  global.Request = jest.fn().mockImplementation(() => ({
    signal: {
      removeEventListener: () => {},
      addEventListener: () => {},
    },
  }));
}

errorOnConsoleError();
