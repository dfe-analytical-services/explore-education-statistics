/* eslint-disable no-console */
import { check, fail } from 'k6';
import { Counter, Rate, Trend } from 'k6/metrics';
import { Options } from 'k6/options';
import exec from 'k6/execution';
import createAdminService from '../../../utils/adminService';
import getOrRefreshAccessTokens from '../../../utils/getOrRefreshAccessTokens';
import getEnvironmentAndUsersFromFile from '../../../utils/environmentAndUsers';

const IMPORT_STATUS_POLLING_DELAY_SECONDS = 5;

const alwaysCreateNewDataPerTest = true;
const bigFile = true;

export const options: Options = {
  stages: [
    {
      duration: '1s',
      target: 3,
    },
    {
      duration: '119m',
      target: 3,
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
  publicationId: string;
  publicationTitle: string;
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

// TODO - use SharedArray instead of `open` here
/* eslint-disable no-restricted-globals */
const zipFile = bigFile ? open('admin/import/assets/big-file1.zip', 'b') : null;
const subjectFile = !bigFile
  ? open('admin/import/assets/big-file.csv', 'b')
  : null;
const subjectMetaFile = !bigFile
  ? open('admin/import/assets/big-file.meta.csv', 'b')
  : null;
/* eslint-enable no-restricted-globals */

const environmentAndUsers = getEnvironmentAndUsersFromFile(
  __ENV.TEST_ENVIRONMENT as string,
);
const { adminUrl, supportsRefreshTokens } = environmentAndUsers.environment;

// eslint-disable-next-line @typescript-eslint/no-non-null-assertion
const { authTokens, userName } = environmentAndUsers.users.find(
  user => user.userName === 'bau1',
)!;

export function setup(): SetupData {
  const adminService = createAdminService(adminUrl, authTokens.accessToken);

  const suffix = alwaysCreateNewDataPerTest
    ? `-${Date.now()}-${Math.random()}`
    : '';

  const { id: themeId } = adminService.getOrCreateTheme({
    title: `UI test theme - Performance tests - "import.test.ts" - ${suffix}`,
  });

  const { id: topicId } = adminService.getOrCreateTopic({
    themeId,
    title: `UI test topic - Performance tests - "import.test.ts" - ${suffix}`,
  });

  const publicationTitle = `UI test publication - Performance tests - "import.test.ts" - ${suffix}`;

  const { id: publicationId } = adminService.getOrCreatePublication({
    topicId,
    title: publicationTitle,
  });

  console.log(
    `Created Theme ${themeId}, Topic ${topicId}, Publication ${publicationId}`,
  );

  return {
    themeId,
    topicId,
    publicationId,
    publicationTitle,
  };
}

const performTest = ({
  topicId,
  publicationId,
  publicationTitle,
}: SetupData) => {
  const accessToken = getOrRefreshAccessTokens(
    supportsRefreshTokens,
    userName,
    adminUrl,
    authTokens,
  );

  const uniqueId = Date.now();
  const subjectName = `subject-${uniqueId}`;

  const adminService = createAdminService(adminUrl, accessToken, false);

  const year = 2000 + exec.scenario.iterationInTest;

  console.log(`Creating Release ${year} for file import to be uploaded to`);

  const { id: releaseId } = adminService.getOrCreateRelease({
    topicId,
    publicationId,
    publicationTitle,
    year,
    timePeriodCoverage: 'AY',
  });

  console.log(`Uploading subject ${subjectName}`);

  const { response: uploadResponse, id: fileId } = bigFile
    ? adminService.uploadDataZipFile({
        title: subjectName,
        releaseId,
        zipFile: {
          file: zipFile!,
          filename: `${subjectName}.zip`,
        },
      })
    : adminService.uploadDataFile({
        title: subjectName,
        releaseId,
        dataFile: {
          file: subjectFile!,
          filename: `${subjectName}.csv`,
        },
        metaFile: {
          file: subjectMetaFile!,
          filename: `${subjectName}.meta.csv`,
        },
      });

  console.log(`Subject ${subjectName} finished uploading`);

  if (
    check(uploadResponse, {
      'response code was 200': res => res.status === 200,
      'response should contain the uploaded file id': _ => !!fileId,
    })
  ) {
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

  adminService.waitForDataFileToImport({
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
          console.log(`Import "${fileId}" - stage ${importStatus} reached`);
        }
      }
    },
    onImportFailed: importStatus => {
      console.log(`Import "${fileId}" - FAILED with status ${importStatus}`);
      errorRate.add(1);
      importFailureCount.add(1);
    },
    onImportCompleted: () => {
      console.log(
        `Import "${fileId}" - COMPLETE after ${
          (Date.now() - importStartTime) / 1000
        } seconds`,
      );
    },
    onImportExceededTimeout: () => {
      console.log(
        `Import "${fileId}" -  EXCEEDED TEST TIMEOUT after ${
          (Date.now() - importStartTime) / 1000
        } seconds`,
      );
      importTimeoutFailureCount.add(1);
    },
  });
};

export const teardown = ({ themeId, topicId }: SetupData) => {
  if (alwaysCreateNewDataPerTest) {
    const accessToken = getOrRefreshAccessTokens(
      supportsRefreshTokens,
      userName,
      adminUrl,
      authTokens,
    );

    const adminService = createAdminService(adminUrl, accessToken);

    adminService.deleteTopic({ topicId });
    adminService.deleteTheme({ themeId });

    console.log(`Deleted Theme ${themeId}, Topic ${topicId}`);
  }
};

export default performTest;
