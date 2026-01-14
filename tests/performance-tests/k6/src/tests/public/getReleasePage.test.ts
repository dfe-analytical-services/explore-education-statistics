/* eslint-disable no-console */
import { Counter, Rate, Trend } from 'k6/metrics';
import { Options } from 'k6/options';
import http from 'k6/http';
import exec from 'k6/execution';
import { check, fail } from 'k6';
import getEnvironmentAndUsersFromFile from '../../utils/environmentAndUsers';
import loggingUtils from '../../utils/loggingUtils';
import steadyRequestRateProfile from '../../configuration/steadyRequestRateProfile';
import spikeProfile from '../../configuration/spikeProfile';
import rampingRequestRateProfile from '../../configuration/rampingRequestRateProfile';
import sequentialRequestProfile from '../../configuration/sequentialRequestsProfile';
import grafanaService from '../../utils/grafanaService';
import { stringifyWithoutNulls } from '../../utils/utils';

const profile = (__ENV.PROFILE ?? 'sequential') as
  | 'load'
  | 'spike'
  | 'stress'
  | 'sequential';

const releasePageUrl =
  __ENV.URL ?? '/find-statistics/pupil-absence-in-schools-in-england/2016-17';
const expectedPublicationTitle =
  __ENV.PUBLICATION_TITLE ?? 'Pupil absence in schools in England';
const expectedContentSnippet =
  __ENV.CONTENT_SNIPPET ?? 'pupils missed on average 8.2 school days';

const urlSlugs = /\/find-statistics\/(.*)\/(.*)/g.exec(releasePageUrl)!;
const publicationSlug = urlSlugs[1];
const releaseSlug = urlSlugs[2];

const dataUrls = [
  `/find-statistics/${publicationSlug}/releases.json?redesign=true&publication=${publicationSlug}`,
  `${releasePageUrl}.json?redesign=true&publication=${publicationSlug}&release=${releaseSlug}`,
  `${releasePageUrl}/explore.json?publication=${publicationSlug}&release=${releaseSlug}&tab=explore`,
  `${releasePageUrl}/methodology.json?publication=${publicationSlug}&release=${releaseSlug}&tab=methodology`,
  `${releasePageUrl}/help.json?publication=${publicationSlug}&release=${releaseSlug}&tab=help`,
  `/find-statistics.json`,
];

export const options = getOptions();

const name = 'getReleasePage.ts';

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

export const errorRate = new Rate('ees_errors');
export const getReleaseSuccessCount = new Counter('ees_get_release_success');
export const getReleaseFailureCount = new Counter('ees_get_release_failure');
export const getReleaseRequestDuration = new Trend(
  'ees_get_release_duration',
  true,
);

export const getReleaseDataRequestDuration = new Trend(
  'ees_get_release_data_duration',
  true,
);
export const getReleaseDataSuccessCount = new Counter(
  'ees_get_release_data_success',
);
export const getReleaseDataFailureCount = new Counter(
  'ees_get_release_data_failure',
);

const environmentAndUsers = getEnvironmentAndUsersFromFile(
  __ENV.TEST_ENVIRONMENT,
);

export function logTestStart(config: Options) {
  console.log(
    `Starting test ${name}, with configuration:\n\n${stringifyWithoutNulls(
      config,
    )}\n\n`,
  );
}

export function setup() {
  loggingUtils.logDashboardUrls();

  logTestStart(exec.test.options);

  grafanaService.testStart({
    name,
    config: exec.test.options,
  });
}

const performTest = () => {
  console.log('Requesting');

  const startTime = Date.now();
  let mainResponse;
  try {
    mainResponse = http.get(
      `${environmentAndUsers.environment.publicUrl}${releasePageUrl}?redesign=true`,
      {
        timeout: '120s',
      },
    );
  } catch (e) {
    getReleaseFailureCount.add(1);
    errorRate.add(1);
    fail(`Failure to get Release page - ${JSON.stringify(e)}`);
  }

  if (
    check(mainResponse, {
      'response code was 200': ({ status }) => status === 200,
      'response should have contained body': ({ body }) => body != null,
      'response contains expected title': res =>
        res.html().text().includes(expectedPublicationTitle),
      'response contains expected content': res =>
        res.html().text().includes(expectedContentSnippet),
    })
  ) {
    // console.log('SUCCESS!');
    getReleaseSuccessCount.add(1);
    getReleaseRequestDuration.add(Date.now() - startTime);
  } else {
    console.log(`FAILURE! Got ${mainResponse.status} response code`);
    getReleaseFailureCount.add(1);
    getReleaseRequestDuration.add(Date.now() - startTime);
    errorRate.add(1);
    fail('Failure to Get Release page');
  }

  const regexp = /"buildId":"([-0-9a-zA-Z]*)"/g;
  const buildId = regexp.exec(mainResponse.body as string)![1];

  dataUrls.forEach(dataUrl => {
    const fullDataUrl = `${environmentAndUsers.environment.publicUrl}/_next/data/${buildId}${dataUrl}`;
    // console.log(`Calling data URL ${fullDataUrl}`);

    let dataResponse;
    try {
      dataResponse = http.get(
        `${environmentAndUsers.environment.publicUrl}${releasePageUrl}?redesign=true`,
        {
          timeout: '120s',
          headers: {
            'x-nextjs-data': '1',
            'x-middleware-prefetch': '1',
            purpose: 'prefetch',
          },
        },
      );
    } catch (e) {
      getReleaseDataFailureCount.add(1);
      errorRate.add(1);
      fail(
        `Failure to get Release data response for URL ${fullDataUrl} - ${JSON.stringify(
          e,
        )}`,
      );
    }

    if (
      check(dataResponse, {
        'response code was 200': ({ status }) => status === 200,
      })
    ) {
      // console.log(`Data call success for URL ${fullDataUrl}`);
      getReleaseDataSuccessCount.add(1);
      getReleaseDataRequestDuration.add(Date.now() - startTime);
    } else {
      console.log(
        `Data call failure! Got ${mainResponse.status} response code for URL ${fullDataUrl}`,
      );
      getReleaseDataFailureCount.add(1);
      getReleaseDataRequestDuration.add(Date.now() - startTime);
      errorRate.add(1);
      fail(`Failure to get Release data response for URL ${fullDataUrl}`);
    }
  });
};
// export function handleSummary(data: unknown) {
//   // return {
//   //   'getReleasePage.html': htmlReport(data),
//   // };
//   return null;
// }
export default performTest;
