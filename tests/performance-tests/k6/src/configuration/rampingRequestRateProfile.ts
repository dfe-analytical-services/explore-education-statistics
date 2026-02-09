import { Options } from 'k6/options';
import { parseFloatOptional, parseIntOptional } from '../utils/utils';

interface Config {
  // Name of the scenario.
  scenario?: string;
  // Total duration of the stage of the test whereby RPS ramps up from zero to the
  // maximum RPS.
  rampingStageDurationMinutes: number;
  // Total duration of the main stage of the test, providing a steady rate of requests.
  mainStageDurationMinutes: number;
  // The maximum requests per second to ramp up to during the main stage.
  maxRequestRatePerSecond: number;
  // Duration of a short final stage to allow any long-running requests to complete.
  cooldownStageDurationMinutes: number;
}

const overrides: Partial<Config> = {
  maxRequestRatePerSecond: parseIntOptional(__ENV.RPS),
  rampingStageDurationMinutes: parseFloatOptional(
    __ENV.RAMPING_TEST_STAGE_DURATION_MINS,
  ),
  mainStageDurationMinutes: parseFloatOptional(
    __ENV.MAIN_TEST_STAGE_DURATION_MINS,
  ),
  cooldownStageDurationMinutes: parseFloatOptional(
    __ENV.MAIN_TEST_STAGE_DURATION_MINS,
  ),
};

export default function rampingRequestRateProfile({
  scenario = 'ramping_rate',
  ...defaultConfig
}: Config & { scenario?: string }): Options {
  const {
    rampingStageDurationMinutes,
    mainStageDurationMinutes,
    maxRequestRatePerSecond,
    cooldownStageDurationMinutes,
  } = {
    ...defaultConfig,
    ...overrides,
  };

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
            duration: `${rampingStageDurationMinutes}m`,
            target: maxRequestRatePerSecond * 60,
          },
          // Maintain the maximum arrival rate for a given duration following
          // on from the ramping up.
          {
            duration: `${mainStageDurationMinutes}m`,
            target: maxRequestRatePerSecond * 60,
          },
          // Immediately drop down to minimal requests for a time to allow
          // long-running requests to complete.
          {
            duration: '0s',
            target: 1,
          },
          // Run with minimal requests for a time to allow long-running requests
          // to complete.
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
