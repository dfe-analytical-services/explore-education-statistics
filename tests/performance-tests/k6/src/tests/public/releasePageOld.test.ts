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
const expectedPublicationTitle =
  __ENV.PUBLICATION_TITLE ?? 'Pupil absence in schools in England';
const expectedContentSnippet =
  __ENV.CONTENT_SNIPPET ?? 'pupils missed on average 8.2 school days';

const environmentAndUsers = getEnvironmentAndUsersFromFile(
  __ENV.TEST_ENVIRONMENT,
);

export const options = getOptions();

const name = 'getReleasePageOld.ts';

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

  const dataUrls: string[] = [
    `/find-statistics.json`,
    `${releasePageUrl}/data-guidance.json?publication=${publicationSlug}&release=${releaseSlug}&tab=explore`,
  ];

  testPageAndDataUrls({
    buildId,
    mainPageUrl: {
      url: releasePageUrl,
      prefetch: false,
      successCounter: getReleaseSuccessCount,
      failureCounter: getReleaseFailureCount,
      durationTrend: getReleaseRequestDuration,
      successCheck: response =>
        check(response, {
          'response code was 200': ({ status }) => status === 200,
          'response should have contained body': ({ body }) => body != null,
          'response contains expected title': res =>
            res.html().text().includes(expectedPublicationTitle),
          'response contains expected content': res =>
            res.html().text().includes(expectedContentSnippet),
        }),
    },
    dataUrls: dataUrls.map(dataUrl => ({
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
  });
};

export default performTest;
