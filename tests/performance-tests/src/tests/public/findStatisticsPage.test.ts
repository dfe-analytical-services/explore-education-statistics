/* eslint-disable no-console */
import { Counter, Rate, Trend } from 'k6/metrics';
import { Options } from 'k6/options';
import http from 'k6/http';
import { check, fail } from 'k6';
import getEnvironmentAndUsersFromFile from '../../utils/environmentAndUsers';
import loggingUtils from '../../utils/loggingUtils';

export const options: Options = {
  stages: [
    {
      duration: '0.1s',
      target: 80,
    },
    {
      duration: '10m',
      target: 80,
    },
  ],
  noConnectionReuse: true,
  insecureSkipTLSVerify: true,
};

export const errorRate = new Rate('ees_errors');
export const getReleaseSuccessCount = new Counter(
  'ees_find_statistics_success',
);
export const getReleaseFailureCount = new Counter(
  'ees_find_statistics_failure',
);
export const getReleaseRequestDuration = new Trend(
  'ees_find_statistics_duration',
  true,
);

const environmentAndUsers = getEnvironmentAndUsersFromFile(
  __ENV.TEST_ENVIRONMENT as string,
);

export function setup() {
  loggingUtils.logDashboardUrls();
}

const performTest = () => {
  const startTime = Date.now();
  let response;
  try {
    response = http.get(
      `${environmentAndUsers.environment.publicUrl}/find-statistics`,
      {
        timeout: '120s',
      },
    );
  } catch (e) {
    getReleaseFailureCount.add(1);
    errorRate.add(1);
    fail(`Failure to get Find Statistics page - ${JSON.stringify(e)}`);
    return;
  }

  if (
    check(response, {
      'response code was 200': ({ status }) => status === 200,
      'response should have contained body': ({ body }) => body != null,
    }) &&
    check(response, {
      'response contains expected text': res =>
        res.html().text().includes('Browse to find the statistics and data'),
    })
  ) {
    console.log('SUCCESS!');
    getReleaseSuccessCount.add(1);
    getReleaseRequestDuration.add(Date.now() - startTime);
  } else {
    console.log(
      `FAILURE!  Got ${response.status} response code - ${JSON.stringify(
        response.body,
      )}`,
    );
    getReleaseFailureCount.add(1);
    getReleaseRequestDuration.add(Date.now() - startTime);
    errorRate.add(1);
    fail('Failure to Find Statistics page');
  }
};

export default performTest;
