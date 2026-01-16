/* eslint-disable no-console */
import { Counter, Trend } from 'k6/metrics';
import { Options } from 'k6/options';
import exec from 'k6/execution';
import { check } from 'k6';
import loggingUtils from '../../utils/loggingUtils';
import grafanaService from '../../utils/grafanaService';
import { stringifyWithoutNulls } from '../../utils/utils';
import getOptions from '../../configuration/options';
import testPageAndDataUrls from './utils/publicPageTest';

const releasePageUrl =
  __ENV.URL ?? '/find-statistics/pupil-absence-in-schools-in-england/2016-17';
const expectedPublicationTitle =
  __ENV.PUBLICATION_TITLE ?? 'Pupil absence in schools in England';
const expectedContentSnippet =
  __ENV.CONTENT_SNIPPET ?? 'pupils missed on average 8.2 school days';

export const options = getOptions();

const name = 'getReleasePage.ts';

const useRedesign = true;

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

export function setup() {
  loggingUtils.logDashboardUrls();

  logTestStart(exec.test.options);

  grafanaService.testStart({
    name,
    config: exec.test.options,
  });
}

const performTest = () => {
  const urlSlugs = /\/find-statistics\/(.*)\/(.*)/g.exec(releasePageUrl)!;
  const publicationSlug = urlSlugs[1];
  const releaseSlug = urlSlugs[2];

  const dataUrls: string[] = [
    `/find-statistics.json`,
    `/find-statistics/${publicationSlug}/releases.json?redesign=true&publication=${publicationSlug}`,
    `${releasePageUrl}.json?redesign=true&publication=${publicationSlug}&release=${releaseSlug}`,
    `${releasePageUrl}/explore.json?publication=${publicationSlug}&release=${releaseSlug}&tab=explore`,
    `${releasePageUrl}/methodology.json?publication=${publicationSlug}&release=${releaseSlug}&tab=methodology`,
    `${releasePageUrl}/help.json?publication=${publicationSlug}&release=${releaseSlug}&tab=help`,
  ];

  testPageAndDataUrls({
    mainPageUrl: {
      url: `${releasePageUrl}${useRedesign ? '?redesign=true' : ''}`,
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
