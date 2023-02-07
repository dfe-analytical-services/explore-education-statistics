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
export const getSubjectMetaSuccessCount = new Counter(
  'ees_get_subject_meta_success',
);
export const getSubjectMetaFailureCount = new Counter(
  'ees_get_subject_meta_failure',
);
export const getSubjectMetaRequestDuration = new Trend(
  'ees_get_subject_meta_duration',
  true,
);

const environmentAndUsers = getEnvironmentAndUsersFromFile(
  __ENV.TEST_ENVIRONMENT as string,
);

export function setup() {
  loggingUtils.logDashboardUrls();
}

const subjectIds = [
  // '16a3cbe9-6352-4f2b-b75a-08daee3941dd',
  // '58f50838-42fd-4d9e-b75c-08daee3941dd',
  // '77acbf19-8c0f-413b-b75e-08daee3941dd',
  'fb58979b-1db9-40ac-b760-08daee3941dd',
];

const releaseId = '9e072b76-84e2-44cd-fdd3-08daee392442';

const performTest = () => {
  http.del(`${environmentAndUsers.environment.dataApiUrl}/buffer-cache`,
  null,
  {
    timeout: '120s',
  });
  const startTime = Date.now();
  const random = Math.floor(Math.random() * subjectIds.length);
  const subjectId = subjectIds[random];
  let response;
  try {
    response = http.get(
      `${environmentAndUsers.environment.dataApiUrl}/release/${releaseId}/meta/subject/${subjectId}`,
      {
        timeout: '120s',
      },
    );
  } catch (e) {
    getSubjectMetaFailureCount.add(1);
    errorRate.add(1);
    fail(`Failure to get Subject meta - ${JSON.stringify(e)}`);
    return;
  }

  if (
    check(response, {
      'response code was 200': ({ status }) => status === 200,
    }) &&
    check(response, {
      'response contains expected fields': res =>
        res.json().timePeriod.hint ===
        'Filter statistics by a given start and end date',
    })
  ) {
    console.log(`'SUCCESS! SubjectId: ${subjectId}`);
    getSubjectMetaSuccessCount.add(1);
    getSubjectMetaRequestDuration.add(Date.now() - startTime);
  } else {
    console.log(
      `FAILURE!  Got ${response.status} response code - ${JSON.stringify(
        response.body,
      )}`,
    );
    getSubjectMetaFailureCount.add(1);
    getSubjectMetaRequestDuration.add(Date.now() - startTime);
    errorRate.add(1);
    fail('Failure to Get Subject meta');
  }
};

export default performTest;
