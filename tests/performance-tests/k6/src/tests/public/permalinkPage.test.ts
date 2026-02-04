import { check } from 'k6';
import { htmlReport } from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js';
import loggingUtils from '../../utils/loggingUtils';
import getStandardOptions from '../../configuration/options';
import testPageAndDataUrls from './utils/publicPageTest';

const name = 'permalinkPage.test.ts';

// slowest permalink on dev at the time of writing
// (after changing max table size to 6 million)
// created using absence by geographic level data
const permalinkId = '010a5b4a-1d1a-4379-4abf-08db14bee00d';
const permalinkTitle = "'1' from 'big permalink'";

export const options = getStandardOptions();

export function setup() {
  loggingUtils.logDashboardUrls();
}

const performTest = () =>
  testPageAndDataUrls({
    mainPageUrl: {
      url: `/data-tables/permalink/${permalinkId}`,
      prefetch: false,
      successCheck: response =>
        check(response, {
          'response code is 200': ({ status }) => status === 200,
          'response should contain body': ({ body }) => body !== null,
          'response contains expected title': res =>
            res.html().text().includes(permalinkTitle),
          'response contains table': res => res.html().find('table') !== null,
        }),
    },
  });

export function handleSummary(data: unknown) {
  return {
    [`${name}.html`]: htmlReport(data),
  };
}

export default performTest;
