/* eslint-disable no-console */
import exec from 'k6/execution';
import { Counter, Rate, Trend } from 'k6/metrics';
import getEnvironmentAndUsersFromFile from '../../../utils/environmentAndUsers';
import loggingUtils from '../../../utils/loggingUtils';
import { isRefinedResponse, pickRandom } from '../../../utils/utils';
import createPublicApiService, {
  DataSetMeta,
  Publication,
} from '../../../utils/publicApiService';
import grafanaService from '../../../utils/grafanaService';
import createQueryGenerator from './queryGenerators';
import config, { Config } from './config';
import {
  logErrorObject,
  logErrorResponse,
  logPublicationsAndDataSets,
  logQueryResponse,
  logTestStart,
} from './logging';

const name = 'publicApiDataSetQuery.test.ts';

export const individualQuerySpeedTrend = new Trend(
  'ees_public_api_individual_query_speed',
  true,
);
export const individualQueryResponseRowsTrend = new Trend(
  'ees_public_api_individual_query_response_rows',
  false,
);
export const individualQueryRequestCount = new Counter(
  'ees_public_api_individual_query_request_count',
);
export const individualQueryCompleteCount = new Counter(
  'ees_public_api_individual_query_complete_count',
);
export const queryFailureCount = new Counter(
  'ees_public_api_query_failure_count',
);
export const errorRate = new Rate('ees_errors');

export const timeoutErrorRate = new Rate('ees_dial_io_timeout_count');

export const connectionRefusedErrorRate = new Rate(
  'ees_public_api_query_connection_refused_count',
);
export const connectionResetErrorRate = new Rate(
  'ees_public_api_query_connection_reset_count',
);
export const http2StreamErrorRate = new Rate(
  'ees_public_api_query_http2_stream_error_count',
);

export interface PublicationAndDataSets extends Publication {
  dataSets: (DataSetMeta & {
    id: string;
    name: string;
  })[];
}

interface SetupData {
  testConfig: Config;
  publications: PublicationAndDataSets[];
}

const environmentAndUsers = getEnvironmentAndUsersFromFile(
  __ENV.TEST_ENVIRONMENT,
);

const { publicApiUrl } = environmentAndUsers.environment;

const publicApiService = createPublicApiService(publicApiUrl);

export const { options, queryConfig, dataSetConfig } = config;

export function setup(): SetupData {
  const testConfig = {
    ...config,
    options: exec.test.options,
  };

  logTestStart(testConfig);

  const { publications } = publicApiService.listPublications();

  const publicationsAndDataSets: PublicationAndDataSets[] = publications.map(
    publication => {
      const { dataSets } = publicApiService.listDataSets(publication.id);

      const dataSetsFilteredByName = dataSets.filter(
        dataSet =>
          !dataSetConfig.limitToTitles ||
          dataSetConfig.limitToTitles.includes(dataSet.title),
      );

      const dataSetMeta = dataSetsFilteredByName.map(dataSet => {
        const { meta } = publicApiService.getDataSetMeta(dataSet.id);
        return {
          ...meta,
          id: dataSet.id,
          name: dataSet.title,
          totalResults: dataSet.latestVersion.totalResults,
        };
      });

      const dataSetsFilteredBySize = dataSetMeta.filter(
        meta =>
          !dataSetConfig.maxRows || meta.totalResults <= dataSetConfig.maxRows,
      );

      return {
        ...publication,
        dataSets: dataSetsFilteredBySize,
      };
    },
  );

  const filteredPublications = publicationsAndDataSets.filter(
    publication => publication.dataSets.length,
  );

  console.log('Found the following Publications and Data Sets:\n\n');
  logPublicationsAndDataSets(filteredPublications);

  loggingUtils.logDashboardUrls();

  grafanaService.testStart({
    name,
    config: testConfig,
  });

  return {
    testConfig,
    publications: filteredPublications,
  };
}

const queryGenerator = createQueryGenerator(queryConfig);

const performTest = ({ publications }: SetupData) => {
  const publication = pickRandom(publications);
  const dataSetMeta = pickRandom(publication.dataSets);

  const query = queryGenerator.generateQuery(dataSetMeta);

  const startTime = Date.now();

  try {
    individualQueryRequestCount.add(1);

    const { results } = publicApiService.queryDataSet({
      dataSetId: dataSetMeta.id,
      query,
    });

    const requestTime = Date.now() - startTime;

    console.log(`Query completed in ${requestTime} ms`);

    individualQuerySpeedTrend.add(requestTime);
    individualQueryCompleteCount.add(1);
    individualQueryResponseRowsTrend.add(results.results.length);

    const totalResponsesTimeMillis = Date.now() - startTime;

    logQueryResponse({
      publication,
      dataSetName: dataSetMeta.name,
      query,
      totalResultsReturned: results.results.length,
      totalPagesReturned: 1,
      totalResponsesTimeMillis,
    });
  } catch (error: unknown) {
    if (isRefinedResponse(error)) {
      if (error.error_code === 1211) {
        timeoutErrorRate.add(1);
        return;
      }

      logErrorResponse(error);

      if (error.error_code === 1212) {
        connectionRefusedErrorRate.add(1);
      }

      if (error.error === 'read: connection reset by peer') {
        connectionResetErrorRate.add(1);
      }

      if (error.error_code >= 1630 && error.error_code <= 1649) {
        http2StreamErrorRate.add(1);
      }

      if (error.status === 400 || error.status === 500) {
        console.log('Error from query endpoint.\n');
        console.log('Metadata was:\n');
        console.log(JSON.stringify(dataSetMeta, null, 2));
        console.log('Query was:\n');
        console.log(JSON.stringify(query, null, 2));
        console.log('Error was:\n');
        console.log(JSON.stringify(error, null, 2));
      }

      queryFailureCount.add(1);
      errorRate.add(1);

      grafanaService.reportErrorResponse({
        name,
        response: error,
      });
    } else {
      logErrorObject(error);

      const url = `/v1/data-sets/${dataSetMeta.id}/query`;

      queryFailureCount.add(1);
      errorRate.add(1);

      grafanaService.reportErrorObject({
        name,
        error,
        url,
        requestBody: query,
      });
    }
  }
};

export const teardown = ({ testConfig }: SetupData) => {
  grafanaService.testStop({
    name,
    config: testConfig,
  });
};

export default performTest;
