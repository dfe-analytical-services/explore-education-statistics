/* eslint-disable no-console */
import { check } from 'k6';
import loggingUtils from '../../utils/loggingUtils';
import getStandardOptions from '../../configuration/options';
import testPageAndDataUrls from './utils/publicPageTest';

// slowest fasttrack on dev at the time of writing
// (after changing max table size to 6 million)
// created using absence by geographic level data
const dataBlockParentId = 'e6fb5d7a-7e21-4256-e59e-08db140dd271';
const fastTrackTableTitle =
  "'1' for Special, State-funded primary and State-funded secondary in Barnsley, Blackburn with Darwen, Blackpool, Bolton, Bradford and 59 other locations between 2006/07 and 2016/17";

export const options = getStandardOptions();

export function setup() {
  loggingUtils.logDashboardUrls();
}

const performTest = () =>
  testPageAndDataUrls({
    mainPageUrl: {
      url: `/data-tables/fast-track/${dataBlockParentId}`,
      prefetch: false,
      successCheck: response =>
        check(response, {
          'response code is 200': ({ status }) => status === 200,
          'response should contain body': ({ body }) => body !== null,
          'response contains expected title': res =>
            res.html().text().includes(fastTrackTableTitle),
          'response contains table': res => res.html().find('table') !== null,
        }),
    },
  });

export default performTest;
