/* eslint-disable no-console */
import { Counter, Trend } from 'k6/metrics';
import { check } from 'k6';
import getOptions from '../../configuration/options';
import testPageAndDataUrls from './utils/publicPageTest';
import setupReleasePageTest, {
  ReleasePageSetupData,
} from './utils/releasePageTest';

const releasePageUrl =
  __ENV.URL ?? '/find-statistics/pupil-absence-in-schools-in-england/2016-17';

export const options = getOptions();

const name = 'releaseMethodologyPage.ts';

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

export function setup(): ReleasePageSetupData {
  return setupReleasePageTest(releasePageUrl, name);
}

const performTest = ({ buildId }: ReleasePageSetupData) => {
  const urlSlugs = /\/find-statistics\/(.*)\/(.*)/g.exec(releasePageUrl)!;
  const publicationSlug = urlSlugs[1];
  const releaseSlug = urlSlugs[2];

  testPageAndDataUrls({
    buildId,
    dataUrls: [
      {
        url: `${releasePageUrl}/explore.json?publication=${publicationSlug}&release=${releaseSlug}&tab=methodology`,
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
