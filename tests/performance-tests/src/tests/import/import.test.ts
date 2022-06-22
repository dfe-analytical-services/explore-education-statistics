import { check, fail, sleep } from 'k6';
import { Counter, Rate, Trend } from 'k6/metrics';
import { Options } from 'k6/options';
import refreshAuthTokens from '../../auth/refreshAuthTokens';
import { AuthDetails, AuthTokens } from '../../auth/getAuthDetails';
import createDataService from '../../utils/dataService';

export const options: Options = {
  // stages: [{ duration: '120m', target: 10 }],
  noConnectionReuse: true,
  insecureSkipTLSVerify: true,
  linger: true,
  vus: 1,
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

export const errorRate = new Rate('ees_errors');
export const importFailureCount = new Counter('ees_import_failure_count');
export const importTimeoutFailureCount = new Counter(
  'ees_import_failure_timeout_count',
);

const processingStageLabels = [
  'QUEUED',
  'STAGE_1',
  'STAGE_2',
  'STAGE_3',
  'STAGE_4',
  'STAGE_4',
  'COMPLETE',
];

const processingStages: {
  [stage: string]: {
    timingMetric: Trend;
    countMetric: Counter;
  };
} = processingStageLabels.reduce(
  (acc, stage) => ({
    ...acc,
    [stage]: {
      timingMetric: new Trend(
        `ees_import_${stage.toLowerCase()}_reached_speed`,
        true,
      ),
      countMetric: new Counter(
        `ees_import_${stage.toLowerCase()}_reached_count`,
      ),
    },
  }),
  {},
);

/* eslint-disable no-restricted-globals */
const subjectFile = open('import/assets/big-files/nd01.csv.csv', 'b');
const subjectMetaFile = open('import/assets/big-files/nd01.meta.csv.csv', 'b');
/* eslint-enable no-restricted-globals */

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
  const dataService = createDataService(adminUrl, authTokens.accessToken);

  const { themeId } = dataService.createTheme({
    title: `UI test theme - Performance tests - "import.test.ts" - ${uniqueId}`,
  });

  const { topicId } = dataService.createTopic({
    themeId,
    title: `UI test topic - Performance tests - "import.test.ts" - ${uniqueId}`,
  });

  const { publicationId } = dataService.createPublication({
    topicId,
    title: `UI test publication - Performance tests - "import.test.ts" - ${uniqueId}`,
  });

  const { releaseId } = dataService.createRelease({
    publicationId,
    releaseName: '2022',
    timePeriodCoverage: 'AY',
  });

  /* eslint-disable-next-line no-console */
  console.log(
    `Created Theme ${themeId}, Topic ${topicId}, Publication ${publicationId}, Release ${releaseId}`,
  );

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
    supportsRefreshTokens,
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

  const dataService = createDataService(adminUrl, accessToken, false);

  const {
    response: uploadResponse,
    fileId,
    importStatus: initialImportStatus,
  } = dataService.importDataFile({
    title: subjectName,
    releaseId,
    dataFile: {
      file: subjectFile,
      filename: `${subjectName}.csv`,
    },
    metaFile: {
      file: subjectMetaFile,
      filename: `${subjectName}.meta.csv`,
    },
  });

  check(uploadResponse, {
    'response code was 200': res => res.status === 200,
    'response should indicate that the uploaded file is queued': _ =>
      initialImportStatus === 'QUEUED',
    'response should contain the uploaded file id': _ => !!fileId,
  });

  const maxImportWaitTimeMillis = 6000 * 1000;
  const importStartTime = Date.now();
  const importExpireTime = importStartTime + maxImportWaitTimeMillis;

  let importComplete = false;

  const processingStagesReported: { [stage: string]: boolean } = Object.keys(
    processingStages,
  ).reduce((acc, [stage]) => ({ ...acc, [stage]: false }), {});

  while (Date.now() < importExpireTime) {
    sleep(1);

    const {
      response: statusResponse,
      importStatus,
    } = dataService.getImportStatus({
      releaseId,
      fileId,
    });

    if (statusResponse.status !== 200) {
      errorRate.add(1);
      importFailureCount.add(1);
      fail(
        `Failure checking on import status of uploaded subject file ${subjectName} - ${JSON.stringify(
          statusResponse.json(),
        )}`,
      );
    }

    if (processingStageLabels.includes(importStatus)) {
      const priorAndCurrentStages = processingStageLabels.slice(
        0,
        processingStageLabels.indexOf(importStatus),
      );

      priorAndCurrentStages.forEach(stage => {
        if (!processingStagesReported[stage]) {
          const { timingMetric, countMetric } = processingStages[stage];
          timingMetric.add(Date.now() - importStartTime);
          countMetric.add(1);
          processingStagesReported[stage] = true;
        }
      });
    }

    if (importStatus === 'FAILED' || importStatus === 'CANCELLED') {
      errorRate.add(1);
      importFailureCount.add(1);
      fail(
        `Failure end state for import process of uploaded subject file ${subjectName} - ${importStatus}`,
      );
      break;
    }

    if (importStatus === 'COMPLETE') {
      importComplete = true;
      break;
    }
  }

  if (!importComplete) {
    errorRate.add(1);
    importTimeoutFailureCount.add(1);
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

  const dataService = createDataService(adminUrl, accessToken);

  dataService.deleteTopic({ topicId });
  dataService.deleteTheme({ themeId });

  /* eslint-disable-next-line no-console */
  console.log(`Deleted Theme ${themeId}, Topic ${topicId}`);
};

export default performTest;
