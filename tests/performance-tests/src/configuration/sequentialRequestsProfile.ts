import { Options } from 'k6/options';
import merge from 'lodash/merge';
import { parseIntOptional } from '../utils/utils';

interface Config {
  // Total duration of the main stage of the test, providing a steady stream
  // of requests one after another, with no concurrency.
  mainStageDurationMinutes?: number;
}

const overrides: Partial<Config> = {
  mainStageDurationMinutes: parseIntOptional(
    __ENV.MAIN_TEST_STAGE_DURATION_MINS,
  ),
};

export default function sequentialRequestProfile(
  defaultConfig: Config,
): Options {
  const { mainStageDurationMinutes } = merge({}, defaultConfig, overrides);
  return {
    duration: `${mainStageDurationMinutes}m`,
    vus: 1,
  };
}
