import { Options } from 'k6/options';
import merge from 'lodash/merge';
import { parseIntOptional } from '../utils/utils';

interface Config {
  // Name of the scenario.
  scenario?: string;
  // Total duration of the main stage of the test, providing a steady rate of requests.
  mainStageDurationMinutes: number;
  // The maximum requests per second to ramp up to during the main stage.
  maxRequestRatePerSecond: number;
  // Duration of a short final stage to allow any long-running requests to complete.
  cooldownStageDurationMinutes: number;
}

const overrides: Partial<Config> = {
  maxRequestRatePerSecond: parseIntOptional(__ENV.RPS),
  mainStageDurationMinutes: parseIntOptional(
    __ENV.MAIN_TEST_STAGE_DURATION_MINS,
  ),
  cooldownStageDurationMinutes: parseIntOptional(
    __ENV.MAIN_TEST_STAGE_DURATION_MINS,
  ),
};

export default function rampingRequestRateProfile({
  scenario = 'ramping_rate',
  ...defaultConfig
}: Config & { scenario?: string }): Options {
  const {
    mainStageDurationMinutes,
    maxRequestRatePerSecond,
    cooldownStageDurationMinutes,
  } = merge({}, defaultConfig, overrides);

  return {
    scenarios: {
      [scenario]: {
        executor: 'ramping-arrival-rate',
        timeUnit: '1m',
        preAllocatedVUs: 100,
        maxVUs: 5000,
        gracefulStop: `${cooldownStageDurationMinutes}m`,
        stages: [
          // Slowly increase traffic up to the maximum arrival rate.
          {
            duration: `${mainStageDurationMinutes}m`,
            target: maxRequestRatePerSecond * 60,
          },
          // Immediately drop down to minimal requests for a time to allow long-running requests to complete.
          {
            duration: '0s',
            target: 1,
          },
          // Run with minimal requests for a time to allow long-running requests to complete.
          {
            duration: `${cooldownStageDurationMinutes}m`,
            target: 1,
          },
        ],
      },
    },
    noConnectionReuse: true,
    insecureSkipTLSVerify: true,
  };
}
