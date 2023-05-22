/* eslint-disable no-console */
import { check } from 'k6';
import exec from 'k6/execution';
import { Counter, Rate, Trend } from 'k6/metrics';
import { Options } from 'k6/options';
import { htmlReport } from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js';
import createAdminService, {
  getDataFileUploadStrategy,
} from '../../../utils/adminService';
import testData from '../../testData';
import getOrRefreshAccessTokens from '../../../utils/getOrRefreshAccessTokens';
import getEnvironmentAndUsersFromFile from '../../../utils/environmentAndUsers';
import createDataService from '../../../utils/dataService';
import loggingUtils from '../../../utils/loggingUtils';
import { createTableBuilderQuery } from '../../../utils/tableQueries';
import { SubjectMeta } from '../../../utils/types';

const tearDownData = false;
const publicationTitle =
  __ENV.PUBLICATION_TITLE ?? 'publicTableBuilderQuery.test.ts';
const dataFile = __ENV.DATA_FILE ?? 'small-file.csv';
const uploadFileStrategy = getDataFileUploadStrategy({
  filename: dataFile,
});

export const options: Options = {
  scenarios: {
    constant_request_rate: {
      executor: 'constant-arrival-rate',
      rate: 10,
      timeUnit: '5s',
      duration: '120m',
      preAllocatedVUs: 10,
      maxVUs: 10,
    },
  },
  noConnectionReuse: true,
  insecureSkipTLSVerify: true,
  setupTimeout: uploadFileStrategy.isZip ? '30m' : '10m',
};

interface SetupData {
  themeId: string;
  topicId: string;
  subjectId: string;
  subjectMeta: SubjectMeta;
}

export const errorRate = new Rate('ees_errors');
export const tableQuerySpeedTrend = new Trend(
  'ees_public_table_query_speed',
  true,
);
export const tableQueryCompleteCount = new Counter(
  'ees_public_table_query_complete_count',
);
export const tableQueryFailureCount = new Counter(
  'ees_public_table_query_failure_count',
);

const environmentAndUsers = getEnvironmentAndUsersFromFile(
  __ENV.TEST_ENVIRONMENT,
);
const {
  adminUrl,
  dataApiUrl,
  supportsRefreshTokens,
} = environmentAndUsers.environment;

// eslint-disable-next-line @typescript-eslint/no-non-null-assertion
const { authTokens, userName } = environmentAndUsers.users.find(
  user => user.userName === 'bau1',
)!;

function getOrCreateReleaseWithSubject() {
  const adminService = createAdminService(
    adminUrl,
    authTokens?.accessToken as string,
  );

  const { id: themeId } = adminService.getOrCreateTheme({
    title: testData.themeName,
  });

  const { id: topicId } = adminService.getOrCreateTopic({
    themeId,
    title: testData.topicName,
  });

  const { id: publicationId } = adminService.getOrCreatePublication({
    topicId,
    title: publicationTitle,
  });

  const { id: releaseId, approvalStatus } = adminService.getOrCreateRelease({
    publicationId,
    publicationTitle,
    topicId,
    year: 2000,
    timePeriodCoverage: 'AY',
  });

  const { subjects: existingSubjects } = adminService.getSubjects({
    releaseId,
  });

  if (!existingSubjects.length) {
    console.log('Importing data file');

    const { id: fileId } = uploadFileStrategy.getOrImportSubject(
      adminService,
      releaseId,
    );

    console.log('Waiting for data file to import');

    adminService.waitForDataFileToImport({
      releaseId,
      fileId,
    });

    console.log('Data file imported successfully');
  }

  const { subjects } = adminService.getSubjects({ releaseId });
  const subjectId = subjects[0].id;

  if (approvalStatus !== 'Approved') {
    adminService.addDataGuidance({
      releaseId,
      subjects: [
        {
          id: subjectId,
          content: '<p>Test</p>',
        },
      ],
    });

    console.log('Approving Release');

    adminService.approveRelease({ releaseId });

    console.log(`Waiting for Release to be published`);

    adminService.waitForReleaseToBePublished({ releaseId });

    console.log(`Release published successfully`);
  }

  return {
    themeId,
    topicId,
    releaseId,
    subjectId,
  };
}

export function setup(): SetupData {
  const adminService = createAdminService(adminUrl, authTokens.accessToken);

  const {
    themeId,
    topicId,
    releaseId,
    subjectId,
  } = getOrCreateReleaseWithSubject();

  const { subjectMeta } = adminService.getSubjectMeta({ releaseId, subjectId });

  loggingUtils.logDashboardUrls();

  return {
    themeId,
    topicId,
    subjectId,
    subjectMeta,
  };
}

const performTest = ({ subjectId, subjectMeta }: SetupData) => {
  const dataService = createDataService(dataApiUrl);

  const query = createTableBuilderQuery({
    subjectId,
    subjectMeta,
  });

  const startTimeMillis = Date.now();

  const { response, results } = dataService.tableQuery(query);

  if (
    check(response, {
      'response code was 200': res => res.status === 200,
      'response should contain table builder results': _ => results.length > 0,
    })
  ) {
    const tableQuerySpeed = Date.now() - startTimeMillis;
    tableQueryCompleteCount.add(1);
    tableQuerySpeedTrend.add(tableQuerySpeed);

    console.log(
      `Table query ${exec.scenario.iterationInTest} completed in ${
        tableQuerySpeed / 1000
      } seconds`,
    );
  } else {
    tableQueryFailureCount.add(1);
    console.log(`Table query ${exec.scenario.iterationInTest} failed`);
  }
};

export const teardown = ({ themeId, topicId }: SetupData) => {
  if (tearDownData) {
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

export function handleSummary(data: unknown) {
  return {
    'publicTableBuilderQuery.html': htmlReport(data),
  };
}

export default performTest;
