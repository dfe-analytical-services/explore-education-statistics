import { Options } from 'k6/options';
import { QueryGeneratorConfig } from './queryGenerators';
import {
  dataSetQueryComparableOperators,
  dataSetQueryIdOperators,
} from '../../../utils/publicApiService';
import steadyRequestRateProfile from '../../../configuration/steadyRequestRateProfile';
import spikeProfile from '../../../configuration/spikeProfile';
import rampingRequestRateProfile from '../../../configuration/rampingRequestRateProfile';
import sequentialRequestProfile from '../../../configuration/sequentialRequestsProfile';
import { parseIntOptional } from '../../../utils/utils';

const profile = (__ENV.PROFILE ?? 'sequential') as
  | 'load'
  | 'spike'
  | 'stress'
  | 'sequential';

const queries = (__ENV.QUERIES ?? 'simple') as 'simple' | 'complex';

const dataSetTitlesToInclude = __ENV.DATA_SET_TITLES_TO_INCLUDE?.split(',');
const dataSetTitlesToExclude = __ENV.DATA_SET_TITLES_TO_EXCLUDE?.split(',');
const maxDataSetRows = parseIntOptional(__ENV.DATA_SET_MAX_ROWS);

function getOptions(): Options {
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
  switch (queries) {
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
      throw Error(`Unknown query configuration '${queries}'`);
  }
}

export interface DataSetConfig {
  includeTitles?: string[];
  excludeTitles?: string[];
  maxRows?: number;
}

export interface Config {
  options: Options;
  queryConfig: QueryGeneratorConfig;
  dataSetConfig: DataSetConfig;
}

const config: Config = {
  options: getOptions(),
  queryConfig: getQueryConfig(),
  dataSetConfig: {
    includeTitles: dataSetTitlesToInclude,
    excludeTitles: dataSetTitlesToExclude,
    maxRows: maxDataSetRows,
  },
};

export default config;
