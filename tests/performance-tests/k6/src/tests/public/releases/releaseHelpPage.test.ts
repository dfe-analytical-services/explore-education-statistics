import getOptions from '../../../configuration/options';
import testPageAndDataUrls, {
  getPagePropsRequestConfig,
  getPrefetchRequestConfig,
  PublicPageSetupData,
  setupPublicPageTest,
} from '../utils/publicPageTest';

const name = 'releaseHelpPage.test.ts';

const releasePageUrl =
  __ENV.URL ??
  '/find-statistics/seed-publication-pupil-absence-in-schools-in-england/2016-17';

const urlSlugs = /\/find-statistics\/(.*)\/(.*)/g.exec(releasePageUrl)!;
const publicationSlug = urlSlugs[1];
const releaseSlug = urlSlugs[2];

export const options = getOptions();

export function setup(): PublicPageSetupData {
  return setupPublicPageTest(name);
}

const performTest = ({ buildId }: PublicPageSetupData) =>
  testPageAndDataUrls({
    buildId,
    dataUrls: [
      // This request occurs on hover-over of the navigation link to the Help tab.
      getPrefetchRequestConfig(
        `${releasePageUrl}/help.json?publication=${publicationSlug}&release=${releaseSlug}&tab=help`,
      ),

      // This request occurs when actually navigating to the Help tab.
      getPagePropsRequestConfig(
        `${releasePageUrl}/help.json?publication=${publicationSlug}&release=${releaseSlug}&tab=help`,
      ),
    ],
  });

export default performTest;
