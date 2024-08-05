import '@testing-library/jest-dom';
import 'core-js/features/array/flat-map';
import 'core-js/features/string/replace-all';
import '@common-test/setupGlobals';
import '@common-test/extend-expect';
import { loadEnvConfig } from '@next/env';
import * as hostUrl from '@common/utils/url/hostUrl';

loadEnvConfig(process.cwd());

jest.setTimeout(10000);
jest.mock('@common/utils/url/hostUrl');
jest
  .spyOn(hostUrl, 'getHostUrl')
  .mockReturnValue(
    new URL('https://explore-education-statistics.servce.gov.uk/'),
  );

if (typeof window !== 'undefined') {
  // fetch polyfill for making API calls.
  require('cross-fetch');
}

global.Request = jest.requireActual('node-fetch').Request;
global.Response = jest.requireActual('node-fetch').Response;
