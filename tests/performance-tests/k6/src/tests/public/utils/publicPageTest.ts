/* eslint-disable no-console */
import { Counter, Rate, Trend } from 'k6/metrics';
import http, { RefinedResponse, ResponseType } from 'k6/http';
import { fail } from 'k6';
import getEnvironmentAndUsersFromFile from '../../../utils/environmentAndUsers';

export const errorRate = new Rate('ees_errors');

const environmentAndUsers = getEnvironmentAndUsersFromFile(
  __ENV.TEST_ENVIRONMENT,
);

const testPageAndDataUrls = ({
  mainPageUrl,
  dataUrls,
  buildId,
}: PublicPageTestConfig & { buildId: string }) => {
  if (mainPageUrl) {
    testRequest(mainPageUrl);
  }

  dataUrls.forEach(dataUrl => {
    testRequest({
      ...dataUrl,
      url: `/_next/data/${buildId}${dataUrl.url}`,
    });
  });
};

function testRequest({
  url,
  prefetch,
  successCheck,
  successCounter,
  failureCounter,
  durationTrend,
}: PublicPageTestUrlConfig): RefinedResponse<ResponseType | undefined> {
  const absoluteUrl = `${environmentAndUsers.environment.publicUrl}${url}`;

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

  return response;
}

export interface PublicPageTestConfig {
  mainPageUrl?: PublicPageTestUrlConfig;
  dataUrls: PublicPageTestUrlConfig[];
}

export interface PublicPageTestUrlConfig {
  url: string;
  prefetch: boolean;
  successCheck: (
    response: RefinedResponse<ResponseType | undefined>,
  ) => boolean;
  successCounter: Counter;
  failureCounter: Counter;
  durationTrend: Trend;
}

export default testPageAndDataUrls;
