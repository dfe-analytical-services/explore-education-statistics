/* eslint-disable no-console */
import exec from 'k6/execution';
import { Counter, Rate, Trend } from 'k6/metrics';
import getEnvironmentAndUsersFromFile from '../../../utils/environmentAndUsers';
import loggingUtils from '../../../utils/loggingUtils';
import { isRefinedResponse, pickRandom } from '../../../utils/utils';
import createPublicApiService, {
  DataSetMeta,
  DataSetQueryRequest,
  Publication,
} from '../../../utils/publicApiService';
import grafanaService from '../../../utils/grafanaService';
import createQueryGenerator from './queryGenerators';
import config, { Config, FixedQuery } from './config';
import {
  logErrorObject,
  logErrorResponse,
  logPublicationsAndDataSets,
  logQuery,
  logQueryResponse,
  logTestStart,
} from './logging';

interface NamedDataSetMeta extends DataSetMeta {
  id: string;
  name: string;
}

interface QueryAndDataSetDetails {
  publicationTitle: string;
  dataSetName: string;
  dataSetId: string;
  query: DataSetQueryRequest;
}

export interface PublicationAndDataSets extends Publication {
  dataSets: NamedDataSetMeta[];
}

type RandomQuerySetupData = {
  queryGenerationMode: 'random';
  publications: PublicationAndDataSets[];
};

type FixedQuerySetupData = {
  queryGenerationMode: 'fixed';
  queries: FixedQuery[];
};

type SetupData = {
  queryGenerationMode: 'fixed' | 'random';
  testConfig: Config;
} & (FixedQuerySetupData | RandomQuerySetupData);

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
export const gatewayTimeoutErrorRate = new Rate(
  'ees_public_api_query_gateway_timeout_error_count',
);

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

  let setupData: SetupData;

  switch (dataSetConfig.queryGenerationMode) {
    case 'random': {
      const { publications } = publicApiService.listPublications();

      const publicationsAndDataSets: PublicationAndDataSets[] =
        publications.map(publication => {
          const { dataSets } = publicApiService.listDataSets(publication.id);

          const dataSetsFilteredByName = dataSets.filter(dataSet => {
            if (dataSetConfig.excludeTitles?.includes(dataSet.title)) {
              console.log(
                `Excluding data set "${dataSet.title}" based on title.`,
              );
              return false;
            }

            if (!dataSetConfig.includeTitles) {
              console.log(`Including data set "${dataSet.title}".`);
              return true;
            }

            if (dataSetConfig.includeTitles.includes(dataSet.title)) {
              console.log(
                `Including data set "${dataSet.title}" based on title.`,
              );
              return true;
            }

            return false;
          });

          const dataSetMeta = dataSetsFilteredByName.map(dataSet => {
            const { meta } = publicApiService.getDataSetMeta(dataSet.id);
            return {
              ...meta,
              id: dataSet.id,
              name: dataSet.title,
              totalResults: dataSet.latestVersion.totalResults,
            };
          });

          const dataSetsFilteredBySize = dataSetMeta.filter(meta => {
            if (
              dataSetConfig.maxRows &&
              meta.totalResults <= dataSetConfig.maxRows
            ) {
              console.log(
                `Excluding data set "${meta.name}" based on max rows.`,
              );
              return false;
            }
            return true;
          });

          return {
            ...publication,
            dataSets: dataSetsFilteredBySize,
          };
        });

      const filteredPublications = publicationsAndDataSets.filter(
        publication => publication.dataSets.length,
      );

      console.log('Found the following Publications and Data Sets:\n\n');
      logPublicationsAndDataSets(filteredPublications);

      setupData = {
        queryGenerationMode: 'random',
        testConfig,
        publications: filteredPublications,
      };
      break;
    }
    case 'fixed': {
      setupData = {
        queryGenerationMode: 'fixed',
        testConfig,
        queries: dataSetConfig.queries,
      };
      break;
    }
    default:
      throw new Error('Unknown query generation strategy');
  }

  loggingUtils.logDashboardUrls();

  grafanaService.testStart({
    name,
    config: testConfig,
  });

  return setupData;
}

const queryGenerator = createQueryGenerator(queryConfig);

const performTest = (setupData: SetupData) => {
  let queryAndDataSet: QueryAndDataSetDetails;
  let dataSetMeta: NamedDataSetMeta | undefined;

  switch (setupData.queryGenerationMode) {
    case 'random': {
      const publication = pickRandom(setupData.publications);
      dataSetMeta = pickRandom(publication.dataSets);
      queryAndDataSet = {
        publicationTitle: publication.title,
        dataSetName: dataSetMeta.name,
        dataSetId: dataSetMeta.id,
        query: queryGenerator.generateQuery(dataSetMeta),
      };
      break;
    }
    case 'fixed': {
      const fixedQuery = pickRandom(setupData.queries);
      queryAndDataSet = fixedQuery;
      break;
    }
    default:
      throw new Error('Unknown query generation strategy');
  }

  logQuery(queryAndDataSet);

  const startTime = Date.now();

  try {
    individualQueryRequestCount.add(1);

    const { results } = publicApiService.queryDataSet({
      dataSetId: queryAndDataSet.dataSetId,
      query: queryAndDataSet.query,
    });

    const requestTime = Date.now() - startTime;

    console.log(`Query completed in ${requestTime} ms`);

    individualQuerySpeedTrend.add(requestTime);
    individualQueryCompleteCount.add(1);
    individualQueryResponseRowsTrend.add(results.results.length);

    const totalResponsesTimeMillis = Date.now() - startTime;

    logQueryResponse({
      publicationTitle: queryAndDataSet.publicationTitle,
      dataSetName: queryAndDataSet.dataSetName,
      query: queryAndDataSet.query,
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

      if (error.error_code === 1504) {
        gatewayTimeoutErrorRate.add(1);
      }

      if (error.status === 400 || error.status === 500) {
        console.log('Error from query endpoint.\n');

        if (dataSetMeta != null) {
          console.log('Metadata was:\n');
          console.log(JSON.stringify(dataSetMeta, null, 2));
        }

        console.log('Query was:\n');
        console.log(JSON.stringify(queryAndDataSet.query, null, 2));
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

      const url = `/v1/data-sets/${queryAndDataSet.dataSetId}/query`;

      queryFailureCount.add(1);
      errorRate.add(1);

      grafanaService.reportErrorObject({
        name,
        error,
        url,
        requestBody: queryAndDataSet.query,
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
