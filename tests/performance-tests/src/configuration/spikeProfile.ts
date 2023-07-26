import { Options } from 'k6/options';
import merge from 'lodash/merge';
import { parseFloatOptional, parseIntOptional } from '../utils/utils';

interface Config {
  // Duration of normal traffic period prior to spike.
  preSpikeStageDurationMinutes: number;
  // Duration of spike traffic period.
  spikeStageDurationMinutes: number;
  // Duration of a short final stage to allow any long-running requests to complete.
  postSpikeStageDurationMinutes: number;
  // Normal traffic levels before and after spike.
  normalTrafficRequestRatePerSecond: number;
  // The maximum requests per second to ramp up to during the traffic spike.
  spikeRequestRatePerSecond: number;
}

const overrides: Partial<Config> = {
  preSpikeStageDurationMinutes: parseFloatOptional(
    __ENV.PRE_SPIKE_DURATION_MINS,
  ),
  spikeStageDurationMinutes: parseFloatOptional(__ENV.SPIKE_DURATION_MINS),
  postSpikeStageDurationMinutes: parseFloatOptional(
    __ENV.POST_SPIKE_DURATION_MINS,
  ),
  normalTrafficRequestRatePerSecond: parseIntOptional(__ENV.RPS_NORMAL),
  spikeRequestRatePerSecond: parseIntOptional(__ENV.RPS_SPIKE),
};

export default function spikeProfile({
  scenario = 'spike',
  ...defaultConfig
}: Config & { scenario?: string }): Options {
  const {
    preSpikeStageDurationMinutes,
    spikeStageDurationMinutes,
    postSpikeStageDurationMinutes,
    normalTrafficRequestRatePerSecond,
    spikeRequestRatePerSecond,
  } = merge({}, defaultConfig, overrides);

  return {
    scenarios: {
      [scenario]: {
        executor: 'ramping-arrival-rate',
        timeUnit: '1m',
        preAllocatedVUs: spikeRequestRatePerSecond * 2,
        maxVUs: spikeRequestRatePerSecond * 2,
        gracefulStop: `${postSpikeStageDurationMinutes}m`,
        stages: [
          // Immediately start with a steady rate of requests.
          {
            duration: '0s',
            target: normalTrafficRequestRatePerSecond * 60,
          },
          // Run with a steady rate of requests for a period.
          {
            duration: `${preSpikeStageDurationMinutes}m`,
            target: normalTrafficRequestRatePerSecond * 60,
          },
          // Very quickly ramp up traffic to a spike of traffic.
          {
            duration: '10s',
            target: spikeRequestRatePerSecond * 60,
          },
          // Continue to run with the spike level of traffic for a short period of time.
          {
            duration: `${spikeStageDurationMinutes}m`,
            target: spikeRequestRatePerSecond * 60,
          },
          // Very quickly drop down to a normal level of traffic.
          {
            duration: '10s',
            target: normalTrafficRequestRatePerSecond * 60,
          },
          // Very quickly drop down to a normal level of traffic.
          {
            duration: `${postSpikeStageDurationMinutes}m`,
            target: normalTrafficRequestRatePerSecond * 60,
          },
        ],
      },
    },
    noConnectionReuse: true,
    insecureSkipTLSVerify: true,
  };
}
