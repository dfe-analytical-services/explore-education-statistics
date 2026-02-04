import { check } from 'k6';
import { htmlReport } from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js';
import getOptions from '../../configuration/options';
import testPageAndDataUrls, {
  PublicPageSetupData,
  setupPublicPageTest,
} from './utils/publicPageTest';

const name = 'releaseHelpPage.test.ts';

const releasePageUrl =
  __ENV.URL ??
  '/find-statistics/seed-publication-pupil-absence-in-schools-in-england/2016-17';

const urlSlugs = /\/find-statistics\/(.*)\/(.*)/g.exec(releasePageUrl)!;
const publicationSlug = urlSlugs[1];
const releaseSlug = urlSlugs[2];

export const options = getOptions();

export function setup(): PublicPageSetupData {
  return setupPublicPageTest(`${releasePageUrl}?redesign=true`, name);
}

const performTest = ({ buildId }: PublicPageSetupData) =>
  testPageAndDataUrls({
    buildId,
    dataUrls: [
      // This request occurs on hover-over of the navigation link to the Help tab.
      {
        url: `${releasePageUrl}/help.json?publication=${publicationSlug}&release=${releaseSlug}&tab=help`,
        prefetch: true,
        successCheck: response =>
          check(response, {
            'response code was 200': ({ status }) => status === 200,
          }),
      },
      // This request occurs when actually navigating to the Help tab.
      {
        url: `${releasePageUrl}/help.json?publication=${publicationSlug}&release=${releaseSlug}&tab=help`,
        prefetch: false,
        successCheck: response =>
          check(response, {
            'response code was 200': ({ status }) => status === 200,
            'response should have contained body': ({ body }) => body != null,
            'response contains pageProps': res => res.json('pageProps') != null,
          }),
      },
    ],
  });

export function handleSummary(data: unknown) {
  return {
    [`${name}.html`]: htmlReport(data),
  };
}

export default performTest;
