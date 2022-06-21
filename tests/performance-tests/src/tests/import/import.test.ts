import { check, fail, sleep } from 'k6';
import http from 'k6/http';
import { Counter, Rate, Trend } from 'k6/metrics';
import { Options } from 'k6/options';
import refreshAuthTokens from '../../auth/refreshAuthTokens';
import { AuthDetails, AuthTokens } from '../../auth/getAuthDetails';

export const options: Options = {
  stages: [{ duration: '60s', target: 10 }],
  noConnectionReuse: true,
  vus: 1,
  insecureSkipTLSVerify: true,
  linger: true,
};

interface SetupData {
  themeId: string;
  topicId: string;
  releaseId: string;
  adminUrl: string;
  userName: string;
  authTokens: AuthTokens;
  supportsRefreshTokens: boolean;
}

export const errorRate = new Rate('errors');
export const importSpeedTrend = new Trend('import_speed', true);
export const importCount = new Counter('import_count');

/* eslint-disable no-restricted-globals */
const subjectFile = open('import/assets/dates.csv', 'b');
const subjectMetaFile = open('import/assets/dates.meta.csv', 'b');
/* eslint-enable no-restricted-globals */

function getHttpParamsWithAuthorization(accessToken: string) {
  return {
    headers: {
      Authorization: `Bearer ${accessToken}`,
      'Content-Type': 'application/json',
    },
  };
}

export function setup(): SetupData {
  const tokenJson = __ENV.AUTH_DETAILS_AS_JSON as string;
  const authDetails = JSON.parse(tokenJson) as AuthDetails[];
  const {
    adminUrl,
    authTokens,
    userName,
    supportsRefreshTokens,
  } = authDetails.find(details => details.userName === 'bau1') as AuthDetails;

  const uniqueId = Date.now();
  const params = getHttpParamsWithAuthorization(authTokens.accessToken);

  const createThemeResponse = http.post(
    `${adminUrl}/api/themes`,
    JSON.stringify({
      title: `UI test theme - "import.test.ts" performance test - ${uniqueId}`,
      summary: '',
    }),
    params,
  );

  if (createThemeResponse.status !== 200) {
    throw new Error(
      `Error creating Theme: ${JSON.stringify(createThemeResponse.json())}`,
    );
  }

  const themeId = ((createThemeResponse.json() as unknown) as { id: string })
    .id;

  const createTopicResponse = http.post(
    `${adminUrl}/api/topics`,
    JSON.stringify({
      themeId,
      title: `UI test topic - "import.test.ts" performance test - ${uniqueId}`,
      summary: '',
    }),
    params,
  );

  if (createTopicResponse.status !== 200) {
    throw new Error(
      `Error creating Topic: ${JSON.stringify(createTopicResponse.json())}`,
    );
  }

  const topicId = ((createTopicResponse.json() as unknown) as { id: string })
    .id;

  const createPublicationResponse = http.post(
    `${adminUrl}/api/publications`,
    JSON.stringify({
      topicId,
      title: `UI test publication - "import" performance test - ${uniqueId}`,
      contact: {
        contactName: 'Team Contact',
        contactTelNo: '12345',
        teamEmail: 'team@example.com',
        teamName: 'Team',
      },
    }),
    params,
  );

  if (createPublicationResponse.status !== 200) {
    throw new Error(
      `Error creating Topic: ${JSON.stringify(
        createPublicationResponse.json(),
      )}`,
    );
  }

  const publicationId = ((createPublicationResponse.json() as unknown) as {
    id: string;
  }).id;

  const createReleaseResponse = http.post(
    `${adminUrl}/api/publications/${publicationId}/releases`,
    JSON.stringify({
      publicationId,
      releaseName: '2021',
      timePeriodCoverage: {
        value: 'AY',
      },
      type: 'NationalStatistics',
    }),
    params,
  );

  if (createReleaseResponse.status !== 200) {
    throw new Error(
      `Error creating Topic: ${JSON.stringify(createReleaseResponse.json())}`,
    );
  }

  const releaseId = ((createReleaseResponse.json() as unknown) as {
    id: string;
  }).id;

  console.log(`Created Theme ${themeId}, Topic ${topicId}`);

  return {
    themeId,
    topicId,
    releaseId,
    userName,
    adminUrl,
    authTokens,
    supportsRefreshTokens,
  };
}

function getOrRefreshAccessToken(
  supportsRefreshTokens: boolean,
  userName: string,
  adminUrl: string,
  authTokens: AuthTokens,
) {
  if (!supportsRefreshTokens) {
    return authTokens.accessToken;
  }

  const refreshedTokens = refreshAuthTokens({
    userName,
    adminUrl,
    clientId: 'GovUk.Education.ExploreEducationStatistics.Admin',
    clientSecret: '',
    refreshToken: authTokens.refreshToken,
  });

  if (!refreshedTokens) {
    throw new Error('Unable to obtain an accessToken - exiting test');
  }

  return refreshedTokens?.authTokens.accessToken;
}

const performTest = ({
  releaseId,
  userName,
  adminUrl,
  authTokens,
  supportsRefreshTokens,
}: SetupData) => {
  const accessToken = getOrRefreshAccessToken(
    supportsRefreshTokens,
    userName,
    adminUrl,
    authTokens,
  );

  const uniqueId = Date.now();
  const subjectName = `dates-${uniqueId}`;

  const params = {
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
  };

  const data = {
    title: `dates-${uniqueId}`,
    file: http.file(subjectFile, `${subjectName}.csv`),
    metaFile: http.file(subjectMetaFile, `${subjectName}.meta.csv`),
  };

  const uploadResponse = http.post(
    `${adminUrl}/api/release/${releaseId}/data?title=${subjectName}`,
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
      `${adminUrl}/api/release/${releaseId}/data/${uploadResponse.json(
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

export const teardown = ({
  supportsRefreshTokens,
  userName,
  adminUrl,
  authTokens,
  themeId,
  topicId,
}: SetupData) => {
  const accessToken = getOrRefreshAccessToken(
    supportsRefreshTokens,
    userName,
    adminUrl,
    authTokens,
  );
  const params = getHttpParamsWithAuthorization(accessToken);

  const deleteTopicResponse = http.del(
    `${adminUrl}/api/topics/${topicId}`,
    null,
    params,
  );

  if (deleteTopicResponse.status !== 204) {
    throw new Error(`Couldn't delete Topic ${topicId}`);
  }

  const deleteThemeResponse = http.del(
    `${adminUrl}/api/themes/${themeId}`,
    null,
    params,
  );

  if (deleteThemeResponse.status !== 204) {
    throw new Error(`Couldn't delete Theme ${topicId}`);
  }

  console.log(`Deleted Theme ${themeId}, Topic ${topicId}`);
};

export default performTest;
