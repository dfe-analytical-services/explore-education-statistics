import { Options } from 'k6/options';
import merge from 'lodash/merge';
import { parseIntOptional } from '../utils/utils';

interface Config {
  // Name of the scenario.
  scenario?: string;
  // Total duration of the main stage of the test, providing a steady rate of requests.
  mainStageDurationMinutes: number;
  // Requests per second to generate during the main stage.
  steadyRequestRatePerSecond: number;
  // Duration of a short final stage to allow any long-running requests to complete.
  cooldownStageDurationMinutes: number;
}

const overrides: Partial<Config> = {
  steadyRequestRatePerSecond: parseIntOptional(__ENV.RPS),
  mainStageDurationMinutes: parseIntOptional(
    __ENV.MAIN_TEST_STAGE_DURATION_MINS,
  ),
  cooldownStageDurationMinutes: parseIntOptional(
    __ENV.MAIN_TEST_STAGE_DURATION_MINS,
  ),
};

export default function steadyRequestRateProfile({
  scenario = 'steady_rate',
  ...defaultConfig
}: Config & { scenario?: string }): Options {
  const {
    steadyRequestRatePerSecond,
    mainStageDurationMinutes,
    cooldownStageDurationMinutes,
  } = merge({}, defaultConfig, overrides);

  return {
    scenarios: {
      [scenario]: {
        executor: 'ramping-arrival-rate',
        timeUnit: '1m',
        preAllocatedVUs: 10,
        maxVUs: 1000,
        gracefulStop: `${cooldownStageDurationMinutes}m`,
        stages: [
          // Immediately start with the steady rate of requests.
          {
            duration: '0s',
            target: steadyRequestRatePerSecond * 60,
          },
          // Continue to run with the steady rate of requests for some time.
          {
            duration: `${mainStageDurationMinutes}m`,
            target: steadyRequestRatePerSecond * 60,
          },
          // Immediately drop down to (nearly) no requests being generated.
          {
            duration: '0s',
            target: 1,
          },
          // Remain running with (nearly) no new requests from being generated to allow any long-running requests to complete.
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
