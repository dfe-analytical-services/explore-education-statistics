import { Options } from 'k6/options';
import steadyRequestRateProfile from './steadyRequestRateProfile';
import spikeProfile from './spikeProfile';
import rampingRequestRateProfile from './rampingRequestRateProfile';
import sequentialRequestProfile from './sequentialRequestsProfile';

export default function getOptions(): Options {
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
        rampingStageDurationMinutes: 1,
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
