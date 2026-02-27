import getStandardOptions from '../../configuration/options';
import testPageAndDataUrls, {
  getPagePropsRequestConfig,
  PublicPageSetupData,
  setupPublicPageTest,
} from './utils/publicPageTest';

const name = 'createYourOwnTablesPageIndirect.test.ts';

export const options = getStandardOptions();

export const setup = () => setupPublicPageTest(name);

const performTest = ({ buildId }: PublicPageSetupData) =>
  testPageAndDataUrls({
    buildId,
    dataUrls: [getPagePropsRequestConfig('/data-tables.json')],
  });

export default performTest;
