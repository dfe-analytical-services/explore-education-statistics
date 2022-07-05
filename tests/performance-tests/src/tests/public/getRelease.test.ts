import {Counter, Rate, Trend} from 'k6/metrics';
import {Options} from 'k6/options';
import http from "k6/http";
import {check, fail} from "k6";

export const options: Options = {
  stages: [
    {
      duration: '0.1s',
      target: 500,
    },
    {
      duration: '10m',
      target: 500,
    },
  ],
  noConnectionReuse: true,
  insecureSkipTLSVerify: true,
  linger: true,
  // vus: 5,
  // duration: '120m',
};

export const errorRate = new Rate('ees_errors');
export const getReleaseSuccessCount = new Counter('ees_get_release_success');
export const getReleaseFailureCount = new Counter('ees_get_release_failure');
export const getReleaseSuccessDuration = new Trend('ees_get_release_duration', true);

interface SetupData {
  publicUrl: string;
}

export function setup(): SetupData {
  // const tokenJson = __ENV.AUTH_DETAILS_AS_JSON as string;
  // const authDetails = JSON.parse(tokenJson) as AuthDetails[];
  // const {publicUrl} = authDetails.find(details => details.userName === 'bau1') as AuthDetails;
  return {
    // publicUrl: 'http://ees.local:3000',
    publicUrl: 'https://xxxx:yyyyy@test.explore-education-statistics.service.gov.uk'
  };
}

const performTest = ({publicUrl}: SetupData) => {

  // const [environment] = process.argv.slice(2);
  //
  // require('dotenv-json-complex')({ environment });
  //
  // const env = JSON.parse(process.env.environment as string) as Environment;

  try {
    const startTime = Date.now();
    const response = http.get(
      // `${publicUrl}/find-statistics/admission-appeals-in-england-user-testing/2019-20`,
      `${publicUrl}/find-statistics/release-cache-blob-test-3-publication/2001-02`,
      {
        timeout: '120s',
      });
    // find-statistics/release-cache-blob-test-publication/2001-02
  
    if (
      check(response, {
        'response code was 200': ({status}) => status === 200,
        'response should have contained body': ({body}) => body != null,
        'response contains expected text': res => res.html().text().includes('Admission appeals in England')
      })
    ) {
      console.log("SUCCESS!");
      getReleaseSuccessCount.add(1);
      getReleaseSuccessDuration.add(Date.now() - startTime);
    } else {
      console.log("FAILURE!");
      getReleaseFailureCount.add(1);
      errorRate.add(1);
    }
  } catch (e) {
    fail(`Failure to get Release page - ${e}`);
    getReleaseFailureCount.add(1);
    errorRate.add(1);
  }
};

export default performTest;
