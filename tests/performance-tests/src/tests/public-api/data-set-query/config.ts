import { Options } from 'k6/options';
import { QueryGeneratorConfig } from './queryGenerators';
import {
  dataSetQueryComparableOperators,
  dataSetQueryIdOperators,
  DataSetQueryRequest,
} from '../../../utils/publicApiService';
import steadyRequestRateProfile from '../../../configuration/steadyRequestRateProfile';
import spikeProfile from '../../../configuration/spikeProfile';
import rampingRequestRateProfile from '../../../configuration/rampingRequestRateProfile';
import sequentialRequestProfile from '../../../configuration/sequentialRequestsProfile';
import { parseIntOptional } from '../../../utils/utils';

export interface FixedQuery {
  publicationTitle: string;
  dataSetName: string;
  dataSetId: string;
  query: DataSetQueryRequest;
}

export interface Config {
  options: Options;
  queryConfig: QueryGeneratorConfig;
  dataSetConfig: DataSetConfig;
}

type RandomDataSetConfig = {
  queryGenerationMode: 'random';
  includeTitles?: string[];
  excludeTitles?: string[];
  maxRows?: number;
};

type FixedDataSetConfig = {
  queryGenerationMode: 'fixed';
  queries: FixedQuery[];
};

type DataSetConfig = {
  queryGenerationMode: 'fixed' | 'random';
} & (RandomDataSetConfig | FixedDataSetConfig);

function getProfileConfig(): Options {
  const profile = (__ENV.PROFILE ?? 'sequential') as
    | 'load'
    | 'spike'
    | 'stress'
    | 'sequential';

  switch (profile) {
    case 'load': {
      return steadyRequestRateProfile({
        mainStageDurationMinutes: 10,
        cooldownStageDurationMinutes: 10,
        steadyRequestRatePerSecond: 10,
      });
    }
    case 'spike': {
      return spikeProfile({
        preSpikeStageDurationMinutes: 1,
        spikeStageDurationMinutes: 1,
        postSpikeStageDurationMinutes: 8,
        normalTrafficRequestRatePerSecond: 10,
        spikeRequestRatePerSecond: 30,
      });
    }
    case 'stress': {
      return rampingRequestRateProfile({
        rampingStageDurationMinutes: 10,
        mainStageDurationMinutes: 10,
        cooldownStageDurationMinutes: 10,
        maxRequestRatePerSecond: 40,
      });
    }
    case 'sequential': {
      return sequentialRequestProfile({
        mainStageDurationMinutes: 10,
      });
    }
    default:
      throw Error(`Unknown profile '${profile}'`);
  }
}

function getQueryConfig(): QueryGeneratorConfig {
  const queryType = (__ENV.QUERY_COMPLEXITY ?? 'simple') as
    | 'simple'
    | 'complex';

  switch (queryType) {
    case 'simple': {
      return {
        idOperators: ['eq', 'in'],
        maxArrayItems: 100,
        maxDepth: 1,
        comparableOperators: [...dataSetQueryComparableOperators],
      };
    }
    case 'complex': {
      return {
        idOperators: [...dataSetQueryIdOperators],
        maxDepth: 2,
        maxBranching: 3,
        comparableOperators: [...dataSetQueryComparableOperators],
      };
    }
    default:
      throw Error(`Unknown query configuration '${queryType}'`);
  }
}

function getDataSetConfig(): DataSetConfig {
  const fixedQueryFiles = __ENV.QUERY_FILES?.split(',');

  if (fixedQueryFiles?.length) {
    const fixedQueries: FixedQuery[] = fixedQueryFiles.map(filename => {
      /* eslint-disable no-restricted-globals */
      const file = open(`public-api/data-set-query/fixed-queries/${filename}`);
      /* eslint-enable no-restricted-globals */
      return JSON.parse(file) as FixedQuery;
    });

    return {
      queryGenerationMode: 'fixed',
      queries: fixedQueries,
    };
  }

  const includeTitles = __ENV.DATA_SET_TITLES_TO_INCLUDE?.split(',');
  const excludeTitles = __ENV.DATA_SET_TITLES_TO_EXCLUDE?.split(',');
  const maxRows = parseIntOptional(__ENV.DATA_SET_MAX_ROWS);

  return {
    queryGenerationMode: 'random',
    includeTitles,
    excludeTitles,
    maxRows,
  };
}

function getConfig(): Config {
  return {
    options: getProfileConfig(),
    queryConfig: getQueryConfig(),
    dataSetConfig: getDataSetConfig(),
  };
}

const config: Config = getConfig();

export default config;
