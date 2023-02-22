import { check, fail } from 'k6';
import http, { RefinedResponse, ResponseType } from 'k6/http';
import { Counter, Rate, Trend } from 'k6/metrics';
import { Options } from 'k6/options';
import getEnvironmentAndUsersFromFile from '../../utils/environmentAndUsers';
import logDashboardUrls from '../../utils/logDashboardUrls';
import logger from '../../utils/logger';

// slowest permalink on dev at the time of writing (after changing max table size to 6 million)
// created using absence by geographic level data
const permalinkId = '010a5b4a-1d1a-4379-4abf-08db14bee00d';
const permalinkTitle = "'1' from 'big permalink'";

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
export const getPermalinkSuccessCount = new Counter(
  'ees_get_permalinkpage_success',
);
export const getPermalinkFailureCount = new Counter(
  'ees_get_permalinkpage_failure',
);
export const getPermalinkPageRequestDuration = new Trend(
  'ees_get_permalinkpage_duration',
  true,
);

const environmentAndUsers = getEnvironmentAndUsersFromFile(
  __ENV.TEST_ENVIRONMENT,
);

export function setup() {
  logDashboardUrls();
}

const performTest = () => {
  const startTime = Date.now();
  let response: RefinedResponse<ResponseType | undefined>;

  try {
    response = http.get(
      `${environmentAndUsers.environment.publicUrl}/data-tables/permalink/${permalinkId}`,
      {
        timeout: '120s',
      },
    );
  } catch (e) {
    getPermalinkFailureCount.add(1);
    errorRate.add(1);
    fail(`Failure to get permalink page - ${JSON.stringify(e)}`);
  }

  if (
    check(response, {
      'response code is 200': ({ status }) => status === 200,
      'response should contain body': ({ body }) => body !== null,
      'response contains expected title': res =>
        res.html().text().includes(permalinkTitle),
      // eslint-disable-next-line react/destructuring-assignment
      'response contains table': res => res.html().find('table') !== null,
    })
  ) {
    logger.info('Passed');
    getPermalinkSuccessCount.add(1);
    getPermalinkPageRequestDuration.add(Date.now() - startTime);
  } else {
    logger.info('Failed');
    getPermalinkFailureCount.add(1);
    errorRate.add(1);
    fail(
      `Failed to get permalink page. Received ${
        response.status
      } response code & body ${JSON.stringify(response.body)}`,
    );
  }
};

export default performTest;
