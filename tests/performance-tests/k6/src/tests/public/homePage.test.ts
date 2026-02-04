import { check } from 'k6';
import { htmlReport } from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js';
import loggingUtils from '../../utils/loggingUtils';
import getStandardOptions from '../../configuration/options';
import testPageAndDataUrls from './utils/publicPageTest';

const name = 'homePage.test.ts';

export const options = getStandardOptions();

export function setup() {
  loggingUtils.logDashboardUrls();
}

const performTest = () =>
  testPageAndDataUrls({
    mainPageUrl: {
      url: '/',
      prefetch: false,
      successCheck: response =>
        check(response, {
          'response code was 200': ({ status }) => status === 200,
          'response should have contained body': ({ body }) => body != null,
          'response contains expected text': res =>
            res.html().text().includes('Explore our statistics and data'),
        }),
    },
  });

export function handleSummary(data: unknown) {
  return {
    [`${name}.html`]: htmlReport(data),
  };
}

export default performTest;
