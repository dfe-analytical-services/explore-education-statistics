import getStandardOptions from '../../configuration/options';
import testPageAndDataUrls, {
  getPagePropsRequestConfig,
  PublicPageSetupData,
  setupPublicPageTest,
} from './utils/publicPageTest';

const name = 'dataCataloguePageIndirect.test.ts';

export const options = getStandardOptions();

export const setup = () => setupPublicPageTest(name);

const performTest = ({ buildId }: PublicPageSetupData) =>
  testPageAndDataUrls({
    buildId,
    dataUrls: [getPagePropsRequestConfig('/data-catalogue.json')],
  });

export default performTest;
