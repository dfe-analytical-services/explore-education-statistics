import { check } from 'k6';
import getStandardOptions from '../../configuration/options';
import testPageAndDataUrls, {
  PublicPageSetupData,
  setupPublicPageTest,
} from './utils/publicPageTest';

const name = 'createYourOwnTablesPageDirect.test.ts';

export const options = getStandardOptions();

export const setup = () => setupPublicPageTest(name);

const performTest = ({ buildId }: PublicPageSetupData) =>
  testPageAndDataUrls({
    buildId,
    mainPageUrl: {
      url: '/data-tables',
      prefetch: false,
      successCheck: response =>
        check(response, {
          'response code was 200': ({ status }) => status === 200,
          'response should have contained body': ({ body }) => body != null,
          'response contains expected text': res =>
            res.html().text().includes('Choose the data and area of interest'),
        }),
    },
  });

export default performTest;
