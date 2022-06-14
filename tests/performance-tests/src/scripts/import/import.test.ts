import { check } from 'k6';
import http from 'k6/http';
import { Rate } from 'k6/metrics';
import refreshAuthTokens from '../../auth/refreshAuthTokens';
import { AuthTokens } from '../../auth/getAuthTokens';

const BASE_URL = 'https://host.docker.internal:5021';

export const options = {
  // stages: [
  //   { duration: '10s', target: 1 },
  // ],
  noConnectionReuse: true,
  linger: true,
  vus: 1,
  timeout: '180s',
  insecureSkipTLSVerify: true,
  httpDebug: 'full',
};

export const errorRate = new Rate('errors');

export function setup() {
  const tokenJson = __ENV.AUTH_TOKENS_AS_JSON as string;
  console.log(tokenJson);
  return JSON.parse(tokenJson) as AuthTokens;
}

// const subjectFile = open('dates.csv', 'b');
// const subjectMetaFile = open('dates.meta.csv', 'b');

export default function ({ access_token, refresh_token }: AuthTokens) {
  const refreshedTokens = refreshAuthTokens({
    baseUrl: BASE_URL,
    clientId: 'GovUk.Education.ExploreEducationStatistics.Admin',
    clientSecret: '',
    refreshToken: refresh_token,
  });

  console.log(refreshedTokens);

  const params = {
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${refreshedTokens!.access_token}`, // or `Bearer ${clientAuthResp.access_token}`
    },
  };

  // const data = {
  //   title: 'subject',
  //   file: http.file(subjectFile, 'dates.csv'),
  //   metaFile: http.file(subjectMetaFile, 'dates.meta.csv'),
  // };

  // const res = http.post(
  //   `${BASE_URL}/api/release/618d7b90-2950-4eff-0f3f-08da49451279/data?title=subject`,
  //     data,
  //     params,
  // );

  const res = http.get(`${BASE_URL}/api/themes`, params);

  console.log('status code is', res.status);
  console.log(`Response time was ${String(res.timings.duration)} ms`);

  check(res, {
    'response code was 200': res => res.status === 200,
  });

  // check(res, {
  //   'response contained the correct text': res =>
  //     res.body.indexOf(
  //       '<h1 class="govuk-heading-xl" data-testid="page-title">Pupil absence in schools in England</h1>',
  //     ) !== -1,
  // });
}
