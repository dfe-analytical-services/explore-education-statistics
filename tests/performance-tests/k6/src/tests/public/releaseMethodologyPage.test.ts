import getOptions from '../../configuration/options';
import testPageAndDataUrls, {
  getPagePropsRequestConfig,
  getPrefetchRequestConfig,
  PublicPageSetupData,
  setupPublicPageTest,
} from './utils/publicPageTest';

const name = 'releaseMethodologyPage.test.ts';

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
      // This request occurs on hover-over of the navigation link to the Methodology tab.
      getPrefetchRequestConfig(
        `${releasePageUrl}/methodology.json?publication=${publicationSlug}&release=${releaseSlug}&tab=methodology`,
      ),

      // This request occurs when actually navigating to the Methodology tab.
      getPagePropsRequestConfig(
        `${releasePageUrl}/methodology.json?publication=${publicationSlug}&release=${releaseSlug}&tab=methodology`,
      ),
    ],
  });

export default performTest;
