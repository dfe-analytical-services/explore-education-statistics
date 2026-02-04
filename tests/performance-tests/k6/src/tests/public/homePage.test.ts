import { check } from 'k6';
import getStandardOptions from '../../configuration/options';
import testPageAndDataUrls, {
  getPrefetchRequestConfig,
  PublicPageSetupData,
  setupPublicPageTest,
} from './utils/publicPageTest';

const name = 'homePage.test.ts';

export const options = getStandardOptions();

const dataUrls = [
  '/find-statistics.json',
  '/data-catalogue.json',
  '/data-tables.json',
  '/methodology.json',
];

export const setup = () => setupPublicPageTest(name);

const performTest = ({ buildId }: PublicPageSetupData) =>
  testPageAndDataUrls({
    buildId,
    mainPageUrl: {
      url: '/',
      prefetch: false,
      successCheck: response =>
        check(response, {
          'response code was 200': ({ status }) => status === 200,
          'response should have contained body': ({ body }) => body != null,
          'response contains expected text': res =>
            res.html().text().includes('Explore our statistics and data'),
        }),
    },
    dataUrls: dataUrls.map(getPrefetchRequestConfig),
  });

// export function handleSummary(data: unknown) {
//   return {
//     [`${name}.html`]: htmlReport(data),
//   };
// }

export default performTest;
