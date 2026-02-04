/* eslint-disable no-console */
import { Counter, Trend } from 'k6/metrics';
import http, { RefinedResponse, ResponseType } from 'k6/http';
import { Options } from 'k6/options';
import execution from 'k6/execution';
import { check, fail } from 'k6';
import getEnvironmentAndUsersFromFile from '../../../utils/environmentAndUsers';
import loggingUtils from '../../../utils/loggingUtils';
import grafanaService from '../../../utils/grafanaService';
import { stringifyWithoutNulls } from '../../../utils/utils';
import {
  getPublicPageDataRequestFailureCount,
  getPublicPageDataRequestDuration,
  getPublicPageDataRequestSuccessCount,
  getPublicPageMainRequestDuration,
  getPublicPageMainRequestFailureCount,
  getPublicPageMainRequestSuccessCount,
  getPublicPageOverallRequestsDuration,
  getPublicPageOverallRequestsSuccessCount,
  getPublicPageOverallRequestsFailureCount,
  getPublicPageOverallDataRequestsSuccessCount,
  getPublicPageOverallDataRequestsDuration,
} from '../publicPageMetrics';
import { errorRate } from '../../../configuration/commonMetrics';

const environmentAndUsers = getEnvironmentAndUsersFromFile(
  __ENV.TEST_ENVIRONMENT,
);

const useCdn = __ENV.USE_CDN?.toLowerCase() === 'true';
const excludeDataUrls = __ENV.EXCLUDE_DATA_REQUESTS?.toLowerCase() === 'true';

export interface PublicPageSetupData {
  buildId: string;
  response: RefinedResponse<ResponseType | undefined>;
}

export function setupPublicPageTest(name: string): PublicPageSetupData {
  loggingUtils.logDashboardUrls();

  console.log(
    `Getting buildId from page ${environmentAndUsers.environment.publicUrl}`,
  );

  const response = http.get(environmentAndUsers.environment.publicUrl);
  const regexp = /"buildId":"([-0-9a-zA-Z_]*)"/g;
  const buildIdMatches = regexp.exec(response.body as string);

  if (!buildIdMatches || buildIdMatches.length === 0) {
    fail(
      `Could not determine Next.JS buildId from page ${environmentAndUsers.environment.publicUrl}`,
    );
  }

  const buildId = buildIdMatches[1];
  console.log(`Found buildId ${buildId}`);

  logTestStart(execution.test.options, name);

  grafanaService.testStart({
    name,
    config: execution.test.options,
  });

  return {
    buildId,
    response,
  };
}

function logTestStart(config: Options, name: string) {
  console.log(
    `Starting test ${name}, with configuration:\n\n${stringifyWithoutNulls(
      config,
    )}\n\n`,
  );
}

const testPageAndDataUrls = ({
  mainPageUrl,
  dataUrls,
  buildId,
}: PublicPageTestConfig & { buildId?: string }) => {
  const overallStartTime = Date.now();

  try {
    if (mainPageUrl) {
      testRequest({
        ...mainPageUrl,
        successCounter: getPublicPageMainRequestSuccessCount,
        failureCounter: getPublicPageMainRequestFailureCount,
        durationTrend: getPublicPageMainRequestDuration,
      });
    }

    if (!excludeDataUrls && dataUrls && dataUrls.length > 0) {
      const dataRequestsStartTime = Date.now();

      dataUrls?.forEach(dataUrl => {
        testRequest({
          ...dataUrl,
          url: `/_next/data/${buildId}${dataUrl.url}`,
          successCounter: getPublicPageDataRequestSuccessCount,
          failureCounter: getPublicPageDataRequestFailureCount,
          durationTrend: getPublicPageDataRequestDuration,
        });
      });

      getPublicPageOverallDataRequestsDuration.add(
        Date.now() - dataRequestsStartTime,
      );
      getPublicPageOverallDataRequestsSuccessCount.add(1);
    }

    getPublicPageOverallRequestsDuration.add(Date.now() - overallStartTime);
    getPublicPageOverallRequestsSuccessCount.add(1);
  } catch (error) {
    getPublicPageOverallRequestsFailureCount.add(1);
    throw error;
  }
};

function testRequest({
  url,
  prefetch,
  successCheck,
  successCounter,
  failureCounter,
  durationTrend,
}: PublicPageTestUrlConfigWithMetrics) {
  const absoluteUrl = useCdn
    ? `${environmentAndUsers.environment.publicCdnUrl}${url}`
    : `${environmentAndUsers.environment.publicUrl}${url}`;

  const startTime = Date.now();
  let response;
  let successfulRequest = false;

  try {
    response = http.get(absoluteUrl, {
      headers: prefetch
        ? {
            'x-nextjs-data': '1',
            'x-middleware-prefetch': '1',
            purpose: 'prefetch',
          }
        : {},
    });

    durationTrend.add(Date.now() - startTime);

    if (!response || response.status === 0) {
      console.error(
        `Transport error: url=${url} error=${response && response.error} code=${
          response && response.error_code
        }`,
      );
    } else {
      successfulRequest = successCheck(response);
    }
  } catch (e) {
    failureCounter.add(1);
    errorRate.add(1);
    fail(`Failure to get page at url ${absoluteUrl}:\n\n${String(e)}`);
  }

  if (successfulRequest) {
    successCounter.add(1);
  } else {
    failureCounter.add(1);
    errorRate.add(1);
    fail(
      `Failure to get page at url ${absoluteUrl}.\nGot response code ${response.status}`,
    );
  }
}

export function getPagePropsRequestConfig(
  url: string,
): PublicPageTestUrlConfig {
  return {
    url,
    prefetch: false,
    successCheck: response =>
      check(response, {
        'response code was 200': ({ status }) => status === 200,
        'response should have contained body': ({ body }) => body != null,
        'response contains pageProps': res => res.json('pageProps') != null,
      }),
  };
}

export function getPrefetchRequestConfig(url: string): PublicPageTestUrlConfig {
  return {
    url,
    prefetch: true,
    successCheck: response =>
      check(response, {
        'response code was 200': ({ status }) => status === 200,
        'body was empty JSON': ({ body }) => body === '{}',
      }),
  };
}

export interface PublicPageTestConfig {
  mainPageUrl?: PublicPageTestUrlConfig;
  dataUrls?: PublicPageTestUrlConfig[];
}

export interface PublicPageTestUrlConfig {
  url: string;
  prefetch: boolean;
  successCheck: (
    response: RefinedResponse<ResponseType | undefined>,
  ) => boolean;
}

type PublicPageTestUrlConfigWithMetrics = PublicPageTestUrlConfig & {
  successCounter: Counter;
  failureCounter: Counter;
  durationTrend: Trend;
};

export default testPageAndDataUrls;
