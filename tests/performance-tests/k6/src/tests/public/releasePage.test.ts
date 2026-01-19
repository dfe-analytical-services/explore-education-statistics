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
const expectedPublicationTitle =
  __ENV.PUBLICATION_TITLE ?? 'Pupil absence in schools in England';
const expectedContentSnippet =
  __ENV.CONTENT_SNIPPET ?? 'pupils missed on average 8.2 school days';

export const options = getOptions();

const name = 'releasePage.ts';

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

export function setup(): ReleasePageSetupData {
  return setupReleasePageTest(releasePageUrl, name);
}

const performTest = ({ buildId }: ReleasePageSetupData) => {
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
    buildId,
    mainPageUrl: {
      url: `${releasePageUrl}?redesign=true`,
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
