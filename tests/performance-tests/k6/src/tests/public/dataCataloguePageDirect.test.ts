/* eslint-disable no-console */
import { check } from 'k6';
import http from 'k6/http';
import getStandardOptions from '../../configuration/options';
import testPageAndDataUrls, {
  getPrefetchRequestConfig,
  PublicPageSetupData,
  setupPublicPageTest,
} from './utils/publicPageTest';
import getEnvironmentAndUsersFromFile from '../../utils/environmentAndUsers';

type SetupData = PublicPageSetupData & {
  dataUrls: string[];
};

const numberOfSearchResultsToPrefetch = 4;

const excludeDataUrls = __ENV.EXCLUDE_DATA_REQUESTS?.toLowerCase() === 'true';

const expectedContent = 'showing all available data sets';

const environmentAndUsers = getEnvironmentAndUsersFromFile(
  __ENV.TEST_ENVIRONMENT,
);

export const options = getStandardOptions();

const name = 'findStatisticsPage.test.ts';

export function setup(): SetupData {
  const { buildId, response } = setupPublicPageTest(name);

  const dataUrls = excludeDataUrls ? [] : getDataUrls();

  return {
    buildId,
    response,
    dataUrls,
  };
}

const performTest = ({ buildId, dataUrls }: SetupData) =>
  testPageAndDataUrls({
    buildId,
    mainPageUrl: {
      url: `/data-catalogue`,
      prefetch: false,
      successCheck: response =>
        check(response, {
          'response code was 200': ({ status }) => status === 200,
          'response should have contained body': ({ body }) => body != null,
          'response contains expected content': res =>
            res.html().text().includes(expectedContent),
        }),
    },
    dataUrls: dataUrls.map(getPrefetchRequestConfig),
  });

/**
 * This function collects the NextJS prefetch URLs that are fired by the top data sets in the search results
 * being in the viewport on page load.  It finds these by interrogating the Content API for search results
 * and building the data URLs from there.
 */
function getDataUrls(): string[] {
  const dataSetSearchResultsUrl = `${environmentAndUsers.environment.contentApiUrl}/data-set-files?page=1&sort=published&sortDirection=Desc`;

  console.log(
    `Getting data set search results from ${dataSetSearchResultsUrl}`,
  );

  const dataSetsJson = http.get(dataSetSearchResultsUrl);

  const dataSets = dataSetsJson.json('results') as {
    publication: {
      slug: string;
    };
    release: {
      slug: string;
    };
  }[];

  const topSearchResultDataSets = dataSets.slice(
    0,
    Math.min(numberOfSearchResultsToPrefetch, dataSets.length),
  );

  // The prefetch URLs for each data set actually fetch details about their owning releases rather than the
  // data sets themselves.
  const dataUrls = topSearchResultDataSets.map(
    r =>
      `/find-statistics/${r.publication.slug}/${r.release.slug}.json?publication=${r.publication.slug}&release=${r.release.slug}`,
  );

  console.log(`Found data URLs: \n\n${dataUrls.join('\n')}`);

  return dataUrls;
}

export default performTest;
