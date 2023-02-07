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
export const filterSubjectMetaSuccessCount = new Counter(
  'ees_filter_subject_meta_success',
);
export const filterSubjectMetaFailureCount = new Counter(
  'ees_filter_subject_meta_failure',
);
export const filterSubjectMetaRequestDuration = new Trend(
  'ees_filter_subject_meta_duration',
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

const locationIds = [
  '0066e760-8759-428e-e24d-08daee3942b8',
  // '00967236-a5d7-40e9-e20a-08daee3942b8',
  // '014d4b65-1e7a-409b-e23b-08daee3942b8',
  // '0181fce5-c84f-4d35-e1bf-08daee3942b8',
  // '02a6d019-7dd6-480a-e1f6-08daee3942b8',
  // '02a8ba5c-15df-433b-e1b2-08daee3942b8',
  // '04a6791a-cbc8-481e-e18a-08daee3942b8',
  // '050346e7-84e5-42d0-e26a-08daee3942b8',
  // '05481158-66d3-40d5-e196-08daee3942b8',
  // '054f6828-7f28-448e-e160-08daee3942b8',
];

const years = [
  // 2017,
  // 2018,
  // 2019,
  2020
];

const releaseId = '9e072b76-84e2-44cd-fdd3-08daee392442';

const performTest = () => {
  const startTime = Date.now();
  const randomSubjectIdIndex = Math.floor(Math.random() * subjectIds.length);
  const randomLocationIdIndex = Math.floor(Math.random() * locationIds.length);
  const randomYearIndex = Math.floor(Math.random() * years.length);
  const subjectId = subjectIds[randomSubjectIdIndex];
  const locationId = locationIds[randomLocationIdIndex];
  const year = years[randomYearIndex];

  const requestBody = {
    subjectId: subjectId,
    indicators: null,
    filters: null,
    locationIds: [locationId],
    timePeriod: {
      startYear: year,
      startCode: "AY",
      endYear: year,
      endCode: "AY"
    },
  };

  let response;
  try {
    response = http.post(
      `${environmentAndUsers.environment.dataApiUrl}/release/${releaseId}/meta/subject`,
      JSON.stringify(requestBody),
      {
        headers: {
          'Content-Type': 'application/json',
        },
        timeout: '120s',
      },
    );
  } catch (e) {
    filterSubjectMetaFailureCount.add(1);
    errorRate.add(1);
    fail(`Failure to filter Subject meta - ${JSON.stringify(e)}`);
    return;
  }

  if (
    check(response, {
      'response code was 200': ({ status }) => status === 200,
    }) &&
    check(response, {
      'response contains expected fields': res =>
        Object.keys(res.json().filters).length > 0,
    })
  ) {
    console.log(`'SUCCESS! SubjectId: ${subjectId} LocationId: ${locationId} Year: ${year}`);
    filterSubjectMetaSuccessCount.add(1);
    filterSubjectMetaRequestDuration.add(Date.now() - startTime);
  } else {
    console.log(
      `FAILURE! SubjectId: ${subjectId} LocationId: ${locationId} Year: ${year} Got ${response.status} response code - ${JSON.stringify(
        response.body,
      )}`,
    );
    filterSubjectMetaFailureCount.add(1);
    filterSubjectMetaRequestDuration.add(Date.now() - startTime);
    errorRate.add(1);
    fail('Failure to Filter Subject meta');
  }
};

export default performTest;
