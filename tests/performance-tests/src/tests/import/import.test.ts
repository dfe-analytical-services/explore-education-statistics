import { check, fail, sleep } from 'k6';
import http from 'k6/http';
import { Counter, Rate, Trend } from 'k6/metrics';
import { Options } from 'k6/options';
import refreshAuthTokens from '../../auth/refreshAuthTokens';
import { AuthDetails } from '../../auth/getAuthDetails';

export const options: Options = {
  stages: [{ duration: '60s', target: 10 }],
  noConnectionReuse: true,
  vus: 1,
  insecureSkipTLSVerify: true,
  linger: true,
};

export const errorRate = new Rate('errors');
export const importSpeedTrend = new Trend('import_speed', true);
export const importCount = new Counter('import_count');

/* eslint-disable no-restricted-globals */
const subjectFile = open('import/assets/dates.csv', 'b');
const subjectMetaFile = open('import/assets/dates.meta.csv', 'b');
/* eslint-enable no-restricted-globals */

const performTest = () => {
  const tokenJson = __ENV.AUTH_DETAILS_AS_JSON as string;
  const originalTokens = JSON.parse(tokenJson) as AuthDetails[];
  const { adminUrl, userName, authTokens } = originalTokens.find(
    details => details.userName === 'bau1',
  ) as AuthDetails;

  const refreshedTokens = refreshAuthTokens({
    userName,
    adminUrl,
    clientId: 'GovUk.Education.ExploreEducationStatistics.Admin',
    clientSecret: '',
    refreshToken: authTokens.refreshToken,
  });

  if (!authTokens) {
    throw new Error('Unable to refresh auth tokens - exiting test');
  }

  const uniqueId = Date.now();
  const subjectName = `dates-${uniqueId}`;

  const params = {
    headers: {
      Authorization: `Bearer ${refreshedTokens?.authTokens.accessToken}`,
    },
  };

  const data = {
    title: `dates-${uniqueId}`,
    file: http.file(subjectFile, `${subjectName}.csv`),
    metaFile: http.file(subjectMetaFile, `${subjectName}.meta.csv`),
  };

  const uploadResponse = http.post(
    `${adminUrl}/api/release/618d7b90-2950-4eff-0f3f-08da49451279/data?title=${subjectName}`,
    data,
    params,
  );

  check(uploadResponse, {
    'response code was 200': res => res.status === 200,
    'response should indicate that the uploaded file is queued': res =>
      res.json('status') === 'QUEUED',
    'response should contain the uploaded file id': res => !!res.json('id'),
  });

  const maxImportWaitTimeMillis = 10 * 1000;
  const importStartTime = Date.now();
  const importExpireTime = importStartTime + maxImportWaitTimeMillis;
  let importComplete = false;

  while (Date.now() < importExpireTime) {
    sleep(1);

    const statusResponse = http.get(
      `${adminUrl}/api/release/618d7b90-2950-4eff-0f3f-08da49451279/data/${uploadResponse.json(
        'id',
      )}/import/status`,
      params,
    );

    if (statusResponse.status !== 200) {
      fail(
        `Failure checking on import status of uploaded subject file ${subjectName} - ${statusResponse.json()}`,
      );
    }

    const importStatus = statusResponse.json('status');

    if (importStatus === 'FAILED' || importStatus === 'CANCELLED') {
      fail(
        `Incorrect end state for import process of uploaded subject file ${subjectName} - ${importStatus}`,
      );
    }

    if (importStatus === 'COMPLETE') {
      importSpeedTrend.add(Date.now() - importStartTime);
      importCount.add(1);
      importComplete = true;
      break;
    }
  }

  if (!importComplete) {
    fail('Failed waiting for file to import');
  }
};

export default performTest;
