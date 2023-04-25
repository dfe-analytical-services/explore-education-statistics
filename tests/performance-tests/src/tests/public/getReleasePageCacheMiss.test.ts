/* eslint-disable no-console */
import { Counter, Rate, Trend } from 'k6/metrics';
import { Options } from 'k6/options';
import http from 'k6/http';
import { check, fail } from 'k6';
import { htmlReport } from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js';
import getEnvironmentAndUsersFromFile from '../../utils/environmentAndUsers';
import loggingUtils from '../../utils/loggingUtils';

export const options: Options = {
  stages: [
    {
      duration: '30s',
      target: 2000,
    },
  ],
  vus: 2000,
  vusMax: 2000,
  noConnectionReuse: true,
  insecureSkipTLSVerify: true,
};

export const errorRate = new Rate('ees_errors');
export const getReleaseSuccessCount = new Counter('ees_get_release_success');
export const getReleaseFailureCount = new Counter('ees_get_release_failure');
export const getReleaseRequestDuration = new Trend(
  'ees_get_release_duration',
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
  let response;
  try {
    response = http.get(
      `${environmentAndUsers.environment.publicUrl}/find-statistics/pupil-absence-in-schools-in-england/2016-17`,
      {
        timeout: '120s',
      },
    );
  } catch (e) {
    getReleaseFailureCount.add(1);
    errorRate.add(1);
    fail(`Failure to get Release page - ${JSON.stringify(e)}`);
  }

  if (
    check(response, {
      'response code was 200': ({ status }) => status === 200,
      'response should have contained body': ({ body }) => body != null,
      'response contains expected title': res =>
        res.html().text().includes('Pupil absence in schools in England'),
      'response contains expected content': res =>
        res.html().text().includes('pupils missed on average 8.2 school days'),
    })
  ) {
    console.log('SUCCESS!');
    getReleaseSuccessCount.add(1);
    getReleaseRequestDuration.add(Date.now() - startTime);
  } else {
    console.log(`FAILURE! Got ${response.status} response code`);
    getReleaseFailureCount.add(1);
    getReleaseRequestDuration.add(Date.now() - startTime);
    errorRate.add(1);
    fail('Failure to Get Release page');
  }
};
export function handleSummary(data: unknown) {
  return {
    'getReleasePageCacheMiss.html': htmlReport(data),
  };
}
export default performTest;
