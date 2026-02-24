import errorOnConsoleError from '@common-test/errorOnConsoleError';
import '@common-test/extend-expect';
import '@common-test/setupGlobals';
import '@testing-library/jest-dom';

jest.setTimeout(10000);

if (typeof window !== 'undefined') {
  require('intersection-observer');
}

errorOnConsoleError();
