import { check } from 'k6';
import { htmlReport } from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js';
import getOptions from '../../configuration/options';
import testPageAndDataUrls, {
  PublicPageSetupData,
  setupPublicPageTest,
} from './utils/publicPageTest';

const name = 'releaseHomePage.test.ts';

const releasePageUrl =
  __ENV.URL ??
  '/find-statistics/seed-publication-pupil-absence-in-schools-in-england/2016-17';
const expectedPublicationTitle =
  __ENV.PUBLICATION_TITLE ??
  'Seed publication - Pupil absence in schools in England';
const expectedContentSnippet =
  __ENV.CONTENT_SNIPPET ?? 'Pupils missed on average 8.2 school days';
const excludeDataUrls = __ENV.EXCLUDE_DATA_REQUESTS?.toLowerCase() === 'true';

const dataUrls: string[] = excludeDataUrls ? [] : [`/find-statistics.json`];

export const options = getOptions();

export function setup(): PublicPageSetupData {
  return setupPublicPageTest(`${releasePageUrl}?redesign=true`, name);
}

const performTest = ({ buildId }: PublicPageSetupData) =>
  testPageAndDataUrls({
    buildId,
    mainPageUrl: {
      url: `${releasePageUrl}?redesign=true`,
      prefetch: false,
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
      successCheck: response =>
        check(response, {
          'response code was 200': ({ status }) => status === 200,
        }),
    })),
  });

export function handleSummary(data: unknown) {
  return {
    [`${name}.html`]: htmlReport(data),
  };
}

export default performTest;
