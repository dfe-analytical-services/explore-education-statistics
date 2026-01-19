/* eslint-disable no-console */
import { Counter, Trend } from 'k6/metrics';
import { check } from 'k6';
import getOptions from '../../configuration/options';
import testPageAndDataUrls from './utils/publicPageTest';
import setupReleasePageTest from './utils/releasePageTest';

interface SetupData {
  buildId: string;
  themeId: string;
  publicationId: string;
  releaseVersionId: string;
}

const releasePageUrl =
  __ENV.URL ?? '/find-statistics/pupil-absence-in-schools-in-england/2016-17';

export const options = getOptions();

const name = 'releaseExplorePage.ts';

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

export function setup(): SetupData {
  const { buildId, response } = setupReleasePageTest(releasePageUrl, name);

  const themeIdRegexp = /"theme":\{"id":"([-0-9a-zA-Z]*)"/g;
  const themeId = themeIdRegexp.exec(response.body as string)![1];

  const publicationIdRegexp = /"publicationSummary":\{"id":"([-0-9a-zA-Z]*)"/g;
  const publicationId = publicationIdRegexp.exec(response.body as string)![1];

  const releaseVersionIdRegexp =
    /"releaseVersionSummary":\{"id":"([-0-9a-zA-Z]*)"/g;
  const releaseVersionId = releaseVersionIdRegexp.exec(
    response.body as string,
  )![1];

  return {
    buildId,
    themeId,
    publicationId,
    releaseVersionId,
  };
}

const performTest = ({
  buildId,
  themeId,
  publicationId,
  releaseVersionId,
}: SetupData) => {
  const urlSlugs = /\/find-statistics\/(.*)\/(.*)/g.exec(releasePageUrl)!;
  const publicationSlug = urlSlugs[1];
  const releaseSlug = urlSlugs[2];

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
        successCounter: getReleaseDataSuccessCount,
        failureCounter: getReleaseDataFailureCount,
        durationTrend: getReleaseDataRequestDuration,
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
