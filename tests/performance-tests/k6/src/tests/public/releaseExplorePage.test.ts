/* eslint-disable no-console */
import { Counter, Trend } from 'k6/metrics';
import { check } from 'k6';
import getOptions from '../../configuration/options';
import testPageAndDataUrls from './utils/publicPageTest';
import setupReleasePageTest from './utils/releasePageTest';

interface SetupData {
  buildId: string;
}

const releasePageUrl =
  __ENV.URL ??
  '/find-statistics/seed-publication-pupil-absence-in-schools-in-england/2016-17';

const urlSlugs = /\/find-statistics\/(.*)\/(.*)/g.exec(releasePageUrl)!;
const publicationSlug = urlSlugs[1];
const releaseSlug = urlSlugs[2];

export const options = getOptions();

const name = 'releaseExplorePage.ts';

export const getReleaseRequestDuration = new Trend(
  'ees_get_release_duration',
  true,
);
export const getReleaseSuccessCount = new Counter('ees_get_release_success');
export const getReleaseFailureCount = new Counter('ees_get_release_failure');
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
  const { buildId } = setupReleasePageTest(releasePageUrl, name);

  return {
    buildId,
  };
}

const performTest = ({ buildId }: SetupData) => {
  const startTime = Date.now();

  try {
    testPageAndDataUrls({
      buildId,
      dataUrls: [
        // This request occurs on hover-over of the navigation link to the Explore tab.
        {
          url: `${releasePageUrl}/explore.json?publication=${publicationSlug}&release=${releaseSlug}&tab=explore`,
          prefetch: true,
          successCounter: getReleaseDataSuccessCount,
          failureCounter: getReleaseDataFailureCount,
          durationTrend: getReleaseDataRequestDuration,
          successCheck: response =>
            check(response, {
              'response code was 200': ({ status }) => status === 200,
            }),
        },
        // This request occurs when actually navigating to the Explore tab.
        {
          url: `${releasePageUrl}/explore.json?publication=${publicationSlug}&release=${releaseSlug}&tab=explore`,
          prefetch: true,
          successCounter: getReleaseDataSuccessCount,
          failureCounter: getReleaseDataFailureCount,
          durationTrend: getReleaseDataRequestDuration,
          successCheck: response =>
            check(response, {
              'response code was 200': ({ status }) => status === 200,
            }),
        },
        // We then see another prefetch request to explore.json when the Explore tab renders.
        {
          url: `${releasePageUrl}/explore.json?publication=${publicationSlug}&release=${releaseSlug}&tab=explore`,
          prefetch: true,
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

    getReleaseRequestDuration.add(Date.now() - startTime);
    getReleaseSuccessCount.add(1);
  } catch (error) {
    getReleaseFailureCount.add(1);
    throw error;
  }
};

export default performTest;
