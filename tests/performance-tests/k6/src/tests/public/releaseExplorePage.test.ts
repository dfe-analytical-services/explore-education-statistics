import { check } from 'k6';
import { htmlReport } from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js';
import getStandardOptions from '../../configuration/options';
import testPageAndDataUrls, {
  setupPublicPageTest,
} from './utils/publicPageTest';

const name = 'releaseExplorePage.test.ts';

interface SetupData {
  buildId: string;
}

const releasePageUrl =
  __ENV.URL ??
  '/find-statistics/seed-publication-pupil-absence-in-schools-in-england/2016-17';

const urlSlugs = /\/find-statistics\/(.*)\/(.*)/g.exec(releasePageUrl)!;
const publicationSlug = urlSlugs[1];
const releaseSlug = urlSlugs[2];

export const options = getStandardOptions();

export function setup(): SetupData {
  const { buildId } = setupPublicPageTest(
    `${releasePageUrl}?redesign=true`,
    name,
  );

  return {
    buildId,
  };
}

const performTest = ({ buildId }: SetupData) => {
  testPageAndDataUrls({
    buildId,
    dataUrls: [
      // This request occurs on hover-over of the navigation link to the Explore tab.
      {
        url: `${releasePageUrl}/explore.json?publication=${publicationSlug}&release=${releaseSlug}&tab=explore`,
        prefetch: true,
        successCheck: response =>
          check(response, {
            'response code was 200': ({ status }) => status === 200,
          }),
      },
      // This request occurs when actually navigating to the Explore tab.
      {
        url: `${releasePageUrl}/explore.json?publication=${publicationSlug}&release=${releaseSlug}&tab=explore`,
        prefetch: false,
        successCheck: response =>
          check(response, {
            'response code was 200': ({ status }) => status === 200,
            'response should have contained body': ({ body }) => body != null,
            'response contains pageProps': res => res.json('pageProps') != null,
          }),
      },
      // We then see another prefetch request to explore.json when the Explore tab renders.
      {
        url: `${releasePageUrl}/explore.json?publication=${publicationSlug}&release=${releaseSlug}&tab=explore`,
        prefetch: true,
        successCheck: response =>
          check(response, {
            'response code was 200': ({ status }) => status === 200,
          }),
      },
    ],
  });
};

export function handleSummary(data: unknown) {
  return {
    [`${name}.html`]: htmlReport(data),
  };
}

export default performTest;
