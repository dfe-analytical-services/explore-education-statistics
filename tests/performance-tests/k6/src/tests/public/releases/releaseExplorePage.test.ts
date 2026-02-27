import testPageAndDataUrls, {
  getPagePropsRequestConfig,
  getPrefetchRequestConfig,
  PublicPageSetupData,
  setupPublicPageTest,
} from '../utils/publicPageTest';
import getStandardOptions from '../../../configuration/options';

const name = 'releaseExplorePage.test.ts';

const releasePageUrl =
  __ENV.URL ??
  '/find-statistics/seed-publication-pupil-absence-in-schools-in-england/2016-17';

const urlSlugs = /\/find-statistics\/(.*)\/(.*)/g.exec(releasePageUrl)!;
const publicationSlug = urlSlugs[1];
const releaseSlug = urlSlugs[2];

export const options = getStandardOptions();

export const setup = () => setupPublicPageTest(name);

const performTest = ({ buildId }: PublicPageSetupData) => {
  testPageAndDataUrls({
    buildId,
    dataUrls: [
      // This request occurs on hover-over of the navigation link to the Explore tab.
      getPrefetchRequestConfig(
        `${releasePageUrl}/explore.json?publication=${publicationSlug}&release=${releaseSlug}&tab=explore`,
      ),

      // This request occurs when actually navigating to the Explore tab.
      getPagePropsRequestConfig(
        `${releasePageUrl}/explore.json?publication=${publicationSlug}&release=${releaseSlug}&tab=explore`,
      ),

      // We then see another prefetch request to explore.json when the Explore tab renders.
      getPrefetchRequestConfig(
        `${releasePageUrl}/explore.json?publication=${publicationSlug}&release=${releaseSlug}&tab=explore`,
      ),
    ],
  });
};

export default performTest;
