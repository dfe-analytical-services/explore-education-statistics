/* eslint-disable no-console */
import { Counter, Trend } from 'k6/metrics';
import { Options } from 'k6/options';
import exec from 'k6/execution';
import { check } from 'k6';
import http from 'k6/http';
import loggingUtils from '../../utils/loggingUtils';
import grafanaService from '../../utils/grafanaService';
import { stringifyWithoutNulls } from '../../utils/utils';
import getOptions from '../../configuration/options';
import testPageAndDataUrls from './utils/publicPageTest';
import getEnvironmentAndUsersFromFile from '../../utils/environmentAndUsers';

interface SetupData {
  buildId: string;
}

const releasePageUrl =
  __ENV.URL ?? '/find-statistics/pupil-absence-in-schools-in-england/2016-17';

const environmentAndUsers = getEnvironmentAndUsersFromFile(
  __ENV.TEST_ENVIRONMENT,
);

export const options = getOptions();

const name = 'getReleasePage.ts';

export const getReleaseSuccessCount = new Counter('ees_get_release_success');
export const getReleaseFailureCount = new Counter('ees_get_release_failure');
export const getReleaseRequestDuration = new Trend(
  'ees_get_release_duration',
  true,
);

export const getReleaseDataRequestDuration = new Trend(
  'ees_get_release_data_duration',
  true,
);
export const getReleaseDataSuccessCount = new Counter(
  'ees_get_release_data_success',
);
export const getReleaseDataFailureCount = new Counter(
  'ees_get_release_data_failure',
);

export function logTestStart(config: Options) {
  console.log(
    `Starting test ${name}, with configuration:\n\n${stringifyWithoutNulls(
      config,
    )}\n\n`,
  );
}

export function setup(): SetupData {
  loggingUtils.logDashboardUrls();

  const response = http.get(
    `${environmentAndUsers.environment.publicUrl}${releasePageUrl}`,
  );
  const regexp = /"buildId":"([-0-9a-zA-Z]*)"/g;
  const buildId = regexp.exec(response.body as string)![1];

  logTestStart(exec.test.options);

  grafanaService.testStart({
    name,
    config: exec.test.options,
  });

  return {
    buildId,
  };
}

const performTest = ({ buildId }: SetupData) => {
  const urlSlugs = /\/find-statistics\/(.*)\/(.*)/g.exec(releasePageUrl)!;
  const publicationSlug = urlSlugs[1];
  const releaseSlug = urlSlugs[2];

  const themeId = '2ca22e34-b87a-4281-a0eb-b80f4f8dd374';
  const publicationId = '24f63a6f-5a5a-4025-d8b5-08d88b0047f4';
  const releaseVersionId = 'e642795f-22ea-4eb6-957b-08d88ad5b210';

  const dataUrls: string[] = [
    `${releasePageUrl}/explore.json?publication=${publicationSlug}&release=${releaseSlug}&tab=explore`,
    `/data-catalogue.json?themeId=${themeId}&publicationId=${publicationId}&releaseVersionId=${releaseVersionId}`,
  ];

  testPageAndDataUrls({
    buildId,
    dataUrls: [
      ...dataUrls.map(dataUrl => ({
        url: dataUrl,
        prefetch: true,
        successCounter: getReleaseDataSuccessCount,
        failureCounter: getReleaseDataFailureCount,
        durationTrend: getReleaseDataRequestDuration,
        successCheck: response =>
          check(response, {
            'response code was 200': ({ status }) => status === 200,
          }),
      })),
      // This request is also in the list of prefetch URLs but is actually a JSON response rather than a prefetch.
      {
        url: `${releasePageUrl}/explore.json?publication=${publicationSlug}&release=${releaseSlug}&tab=explore`,
        prefetch: false,
        successCounter: getReleaseSuccessCount,
        failureCounter: getReleaseFailureCount,
        durationTrend: getReleaseRequestDuration,
        successCheck: response =>
          check(response, {
            'response code was 200': ({ status }) => status === 200,
            'response should have contained body': ({ body }) => body != null,
            'response contains expected content': res =>
              res.html().text().includes('pageProps'),
          }),
      },
    ],
  });
};

export default performTest;
