/* eslint-disable no-console */
import { Counter, Rate, Trend } from 'k6/metrics';
import { Options } from 'k6/options';
import http from 'k6/http';
import { check, fail } from 'k6';
import getEnvironmentAndUsersFromFile from '../../utils/environmentAndUsers';
import loggingUtils from '../../utils/loggingUtils';

export const options: Options = {
  // stages: [
  //   {
  //     duration: '0.1s',
  //     target: 5,
  //   },
  //   {
  //     duration: '1m',
  //     target: 80,
  //   },
  // ],
  // iterations: 10000,
  noConnectionReuse: false,
  insecureSkipTLSVerify: true,
  linger: false,
  vus: 1,
  duration: '10m',
};

export const errorRate = new Rate('ees_errors');
export const listSubjectsSuccessCount = new Counter(
  'ees_list_subjects_success',
);
export const listSubjectsFailureCount = new Counter(
  'ees_list_subjects_failure',
);
export const listSubjectsRequestDuration = new Trend(
  'ees_list_subjects_duration',
  true,
);

const environmentAndUsers = getEnvironmentAndUsersFromFile(
  __ENV.TEST_ENVIRONMENT as string,
);

export function setup() {
  loggingUtils.logDashboardUrls();
}

const releaseId = '9e072b76-84e2-44cd-fdd3-08daee392442';

const performTest = () => {
  http.del(`${environmentAndUsers.environment.dataApiUrl}/buffer-cache`,
  null,
  {
    timeout: '120s',
  });

  const startTime = Date.now();
  let response;
  try {
    response = http.get(
      `${environmentAndUsers.environment.dataApiUrl}/releases/${releaseId}/subjects`,
      {
        timeout: '120s',
      },
    );
  } catch (e) {
    listSubjectsFailureCount.add(1);
    errorRate.add(1);
    fail(`Failure to list subjects - ${JSON.stringify(e)}`);
    return;
  }

  if (
    check(response, {
      'response code was 200': ({ status }) => status === 200,
    }) &&
    check(response, {
      'response contains expected fields': res =>
        res.json()[0].id === '16a3cbe9-6352-4f2b-b75a-08daee3941dd',
    })
  ) {
    console.log('SUCCESS!');
    listSubjectsSuccessCount.add(1);
    listSubjectsRequestDuration.add(Date.now() - startTime);
  } else {
    console.log(
      `FAILURE!  Got ${response.status} response code - ${JSON.stringify(
        response.body,
      )}`,
    );
    listSubjectsFailureCount.add(1);
    listSubjectsRequestDuration.add(Date.now() - startTime);
    errorRate.add(1);
    fail('Failure to List subjects');
  }
};

export default performTest;
