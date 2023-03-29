/* eslint-disable no-console */
import { RefinedResponse } from 'k6/http';
import { stringifySimplifiedQuery } from './queryGenerators';
import {
  DataSetQueryRequest,
  Publication,
} from '../../../utils/publicApiService';
import { Config } from './config';
import { PublicationAndDataSets } from './publicApiDataSetQuery.test';
import { stringifyWithoutNulls } from '../../../utils/utils';

const logQueries = __ENV.LOG_QUERIES === 'true';
const logResponseSummaries = __ENV.LOG_RESPONSE_SUMMARIES === 'true';

const name = 'publicApiDataSetQuery.test.ts';

export function logTestStart(config: Config) {
  console.log(
    `Starting test ${name}, with configuration:\n\n${stringifyWithoutNulls(
      config,
    )}\n\n`,
  );
}

export function logPublicationsAndDataSets(
  publications: PublicationAndDataSets[],
) {
  publications.forEach(publication => {
    console.log(`  - ${publication.title}`);
    publication.dataSets.forEach(dataSet =>
      console.log(`    - ${dataSet.name} (${dataSet.totalResults} results)`),
    );
  });
}

export function logQueryResponse({
  publication,
  dataSetName,
  query,
  totalResultsReturned,
  totalPagesReturned,
  totalResponsesTimeMillis,
}: {
  publication: Publication;
  dataSetName: string;
  query: DataSetQueryRequest;
  totalResultsReturned: number;
  totalPagesReturned: number;
  totalResponsesTimeMillis: number;
}) {
  if (logResponseSummaries) {
    console.log(`\n\n=============================`);
    console.log(`Publication:  ${publication.title}`);
    console.log(`Data Set:     ${dataSetName}\n`);

    if (logQueries) {
      console.log(`Query:      ${stringifySimplifiedQuery(query)}`);
    }

    if (totalResultsReturned === 0) {
      console.log(`Results:    No results matched query`);
    } else {
      console.log(
        `Results:    ${totalResultsReturned} over ${totalPagesReturned} pages`,
      );
    }

    console.log(`Total time:   ${totalResponsesTimeMillis / 1000} seconds`);
  }
}

export function logErrorResponse(response: RefinedResponse<'text'>) {
  console.log(`Error response: ${response.error_code} - ${response.error}`);
}

export function logErrorObject(error: unknown) {
  console.log(`Error response: ${JSON.stringify(error)}`);
}
