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

const expectedTitle = 'Find statistics and data';
const excludeDataUrls = __ENV.EXCLUDE_DATA_REQUESTS?.toLowerCase() === 'true';

const environmentAndUsers = getEnvironmentAndUsersFromFile(
  __ENV.TEST_ENVIRONMENT,
);

export const options = getStandardOptions();

const name = 'findStatisticsPage.test.ts';

export function setup(): SetupData {
  const { buildId, response } = setupPublicPageTest(name);

  let dataUrls: string[];

  if (!excludeDataUrls) {
    const defaultSearchResultsJson = http.get(
      `${environmentAndUsers.environment.publicUrl}/_next/data/${buildId}/find-statistics.json`,
    );

    const publications = defaultSearchResultsJson.json(
      'pageProps.dehydratedState.queries.0.state.data.results',
    ) as {
      slug: string;
      latestReleaseSlug: string;
    }[];

    const topSearchResultPublications = publications.slice(
      0,
      Math.min(4, publications.length),
    );

    dataUrls = topSearchResultPublications.map(
      p =>
        `/find-statistics/${p.slug}/${p.latestReleaseSlug}.json?publication=${p.slug}&release=${p.latestReleaseSlug}`,
    );

    console.log(`Found data URLs: \n\n${dataUrls.join('\n')}`);
  } else {
    dataUrls = [];
  }

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
      url: `/find-statistics`,
      prefetch: false,
      successCheck: response =>
        check(response, {
          'response code was 200': ({ status }) => status === 200,
          'response should have contained body': ({ body }) => body != null,
          'response contains expected title': res =>
            res.html().text().includes(expectedTitle),
        }),
    },
    dataUrls: dataUrls.map(getPrefetchRequestConfig),
  });

export default performTest;
