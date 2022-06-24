import { check, fail } from 'k6';
import { Counter, Rate, Trend } from 'k6/metrics';
import { Options } from 'k6/options';
import { AuthDetails, AuthTokens } from '../../auth/getAuthDetails';
import createDataService from '../../utils/dataService';
import getOrRefreshAccessTokens from '../../utils/getOrRefreshAccessTokens';

const IMPORT_STATUS_POLLING_DELAY_SECONDS = 5;

const alwaysCreateNewDataPerTest = false;

export const options: Options = {
  stages: [
    {
      duration: '1s',
      target: 2,
    },
    {
      duration: '119m',
      target: 2,
    },
    {
      duration: '30s',
      target: 0,
    },
  ],
  noConnectionReuse: true,
  insecureSkipTLSVerify: true,
  linger: true,
  // vus: 5,
  // duration: '120m',
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
  'COMPLETE',
];

const processingStages: {
  [processingStage: string]: {
    timingMetric: Trend;
    countMetric: Counter;
  };
} = processingStageLabels.reduce(
  (acc, processingStage) => ({
    ...acc,
    [processingStage]: {
      timingMetric: new Trend(
        `ees_import_${processingStage.toLowerCase()}_reached_speed`,
        true,
      ),
      countMetric: new Counter(
        `ees_import_${processingStage.toLowerCase()}_reached_count`,
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

  const dataService = createDataService(adminUrl, authTokens.accessToken);

  const suffix = alwaysCreateNewDataPerTest
    ? `-${Date.now()}-${Math.random()}`
    : '';

  const { id: themeId } = dataService.getOrCreateTheme({
    title: `UI test theme - Performance tests - "import.test.ts" - ${suffix}`,
  });

  const { id: topicId } = dataService.getOrCreateTopic({
    themeId,
    title: `UI test topic - Performance tests - "import.test.ts" - ${suffix}`,
  });

  const publicationTitle = `UI test publication - Performance tests - "import.test.ts" - ${suffix}`;

  const { id: publicationId } = dataService.getOrCreatePublication({
    topicId,
    title: publicationTitle,
  });

  const { id: releaseId } = dataService.getOrCreateRelease({
    topicId,
    publicationId,
    publicationTitle,
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

const performTest = ({
  releaseId,
  userName,
  adminUrl,
  authTokens,
  supportsRefreshTokens,
}: SetupData) => {
  const accessToken = getOrRefreshAccessTokens(
    supportsRefreshTokens,
    userName,
    adminUrl,
    authTokens,
  );

  const uniqueId = Date.now();
  const subjectName = `dates-${uniqueId}`;

  const dataService = createDataService(adminUrl, accessToken, false);

  /* eslint-disable-next-line no-console */
  console.log(`Uploading subject ${subjectName}`);

  const { response: uploadResponse, id: fileId } = dataService.uploadDataFile({
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

  /* eslint-disable-next-line no-console */
  console.log(`Subject ${subjectName} finished uploading`);

  if (
    check(uploadResponse, {
      'response code was 200': res => res.status === 200,
      'response should contain the uploaded file id': _ => !!fileId,
    })
  ) {
    /* eslint-disable-next-line no-console */
    console.log(`Subject ${subjectName} finished uploading`);
  } else {
    fail(
      `Subject ${subjectName} failed to upload successfully - got response ${JSON.stringify(
        uploadResponse.json(),
      )}`,
    );
  }

  // Mark each processing stage as not having been reported yet.
  const processingStagesReported: { [stage: string]: boolean } = Object.keys(
    processingStages,
  ).reduce((acc, [stage]) => ({ ...acc, [stage]: false }), {});

  const importStartTime = Date.now();

  dataService.waitForDataFileToImport({
    releaseId,
    fileId,
    pollingDelaySeconds: IMPORT_STATUS_POLLING_DELAY_SECONDS,
    onStatusCheckFailed: _ => {
      errorRate.add(1);
    },
    onStatusReceived: importStatus => {
      if (processingStageLabels.includes(importStatus)) {
        const priorAndCurrentStages = processingStageLabels.slice(
          0,
          processingStageLabels.indexOf(importStatus) + 1,
        );

        const unreportedStages = priorAndCurrentStages.filter(
          stage => !processingStagesReported[stage],
        );

        unreportedStages.forEach(stage => {
          const { timingMetric, countMetric } = processingStages[stage];
          timingMetric.add(Date.now() - importStartTime);
          countMetric.add(1);
          processingStagesReported[stage] = true;
        });

        if (unreportedStages.length) {
          /* eslint-disable-next-line no-console */
          console.log(`Import "${fileId}" - stage ${importStatus} reached`);
        }
      }
    },
    onImportFailed: importStatus => {
      /* eslint-disable-next-line no-console */
      console.log(`Import "${fileId}" - FAILED with status ${importStatus}`);
      errorRate.add(1);
      importFailureCount.add(1);
    },
    onImportCompleted: () => {
      /* eslint-disable-next-line no-console */
      console.log(
        `Import "${fileId}" - COMPLETE after ${
          (Date.now() - importStartTime) / 1000
        } seconds`,
      );
    },
    onImportExceededTimeout: () => {
      /* eslint-disable-next-line no-console */
      console.log(
        `Import "${fileId}" -  EXCEEDED TEST TIMEOUT after ${
          (Date.now() - importStartTime) / 1000
        } seconds`,
      );
      importTimeoutFailureCount.add(1);
    },
  });
};

export const teardown = ({
  supportsRefreshTokens,
  userName,
  adminUrl,
  authTokens,
  themeId,
  topicId,
}: SetupData) => {
  if (alwaysCreateNewDataPerTest) {
    const accessToken = getOrRefreshAccessTokens(
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
  }
};

export default performTest;
