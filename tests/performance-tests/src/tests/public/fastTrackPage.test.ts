/* eslint-disable no-console */
import { check, fail } from 'k6';
import http, { RefinedResponse, ResponseType } from 'k6/http';
import { Counter, Rate, Trend } from 'k6/metrics';
import { Options } from 'k6/options';
import { htmlReport } from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js';
import getEnvironmentAndUsersFromFile from '../../utils/environmentAndUsers';
import loggingUtils from '../../utils/loggingUtils';

// slowest fasttrack on dev at the time of writing
// (after changing max table size to 6 million)
// created using absence by geographic level data
const fastTrackId = 'e6fb5d7a-7e21-4256-e59e-08db140dd271';
const fastTrackTableTitle =
  "'1' for Special, State-funded primary and State-funded secondary in Barnsley, Blackburn with Darwen, Blackpool, Bolton, Bradford and 59 other locations between 2006/07 and 2016/17";

export const options: Options = {
  stages: [
    {
      duration: '0.1s',
      target: 50,
    },
    {
      duration: '10m',
      target: 40,
    },
  ],
  noConnectionReuse: true,
  insecureSkipTLSVerify: true,
};

export const errorRate = new Rate('ees_errors');
export const getfastTrackSuccessCount = new Counter(
  'ees_get_fasttrackpage_success',
);
export const getfastTrackFailureCount = new Counter(
  'ees_get_fasttrackpage_failure',
);
export const getfastTrackPageRequestDuration = new Trend(
  'ees_get_fasttrackpage_duration',
  true,
);
const environmentAndUsers = getEnvironmentAndUsersFromFile(
  __ENV.TEST_ENVIRONMENT,
);

export function setup() {
  loggingUtils.logDashboardUrls();
}

const performTest = () => {
  const startTime = Date.now();
  let response: RefinedResponse<ResponseType | undefined>;

  try {
    response = http.get(
      `${environmentAndUsers.environment.publicUrl}/data-tables/fast-track/${fastTrackId}`,
      {
        timeout: '120s',
      },
    );
  } catch (e) {
    getfastTrackFailureCount.add(1);
    errorRate.add(1);
    fail(`Failure to get fast track page - ${JSON.stringify(e)}`);
  }

  if (
    check(response, {
      'response code is 200': ({ status }) => status === 200,
      'response should contain body': ({ body }) => body !== null,
      'response contains expected title': res =>
        res.html().text().includes(fastTrackTableTitle),
      // eslint-disable-next-line react/destructuring-assignment
      'response contains table': res => res.html().find('table') !== null,
    })
  ) {
    console.log('Passed');
    getfastTrackSuccessCount.add(1);
    getfastTrackPageRequestDuration.add(Date.now() - startTime);
  } else {
    console.log('Failed');
    getfastTrackFailureCount.add(1);
    errorRate.add(1);
    fail(
      `Failed to get fast track page. Received ${response.status} response code`,
    );
  }
};
export function handleSummary(data: unknown) {
  return {
    'fastTrackPage.html': htmlReport(data),
  };
}
export default performTest;
