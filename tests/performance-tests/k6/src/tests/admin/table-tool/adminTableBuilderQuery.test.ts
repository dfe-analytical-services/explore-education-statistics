/* eslint-disable no-restricted-globals */
/* eslint-disable no-console */
import { check } from 'k6';
import { Counter, Rate, Trend } from 'k6/metrics';
import { Options } from 'k6/options';
import { htmlReport } from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js';
import exec from 'k6/execution';
import createAdminService, {
  getDataFileUploadStrategy,
} from '../../../utils/adminService';
import testData from '../../testData';
import getOrRefreshAccessTokens from '../../../utils/getOrRefreshAccessTokens';
import getEnvironmentAndUsersFromFile from '../../../utils/environmentAndUsers';
import loggingUtils from '../../../utils/loggingUtils';
import { createTableBuilderQuery } from '../../../utils/tableQueries';
import { SubjectMeta } from '../../../utils/types';

const tearDownData = false;
const publicationTitle =
  __ENV.PUBLICATION_TITLE ?? 'adminTableBuilderQuery.test.ts';
const dataFile = __ENV.DATA_FILE ?? 'small-file.csv';
const uploadFileStrategy = getDataFileUploadStrategy({
  filename: dataFile,
});

export const options: Options = {
  scenarios: {
    constant_request_rate: {
      executor: 'constant-arrival-rate',
      rate: 1,
      timeUnit: '5s',
      duration: '120m',
      preAllocatedVUs: 3,
      maxVUs: 10,
    },
  },
  noConnectionReuse: true,
  insecureSkipTLSVerify: true,
  setupTimeout: uploadFileStrategy.isZip ? '30m' : '10m',
};

interface SetupData {
  themeId: string;
  releaseId: string;
  subjectId: string;
  subjectMeta: SubjectMeta;
}

export const errorRate = new Rate('ees_errors');
export const tableQuerySpeedTrend = new Trend(
  'ees_admin_table_query_speed',
  true,
);
export const tableQueryCompleteCount = new Counter(
  'ees_admin_table_query_complete_count',
);
export const tableQueryFailureCount = new Counter(
  'ees_admin_table_query_failure_count',
);

const environmentAndUsers = getEnvironmentAndUsersFromFile(
  __ENV.TEST_ENVIRONMENT,
);
const { adminUrl } = environmentAndUsers.environment;

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

  const { id: publicationId } = adminService.getOrCreatePublication({
    themeId,
    title: publicationTitle,
  });

  const { id: releaseId } = adminService.getOrCreateRelease({
    publicationId,
    publicationTitle,
    themeId,
    year: 2022,
    timePeriodCoverage: 'AY',
  });

  const { subjects: existingSubjects } = adminService.getSubjects({
    releaseId,
  });

  if (!existingSubjects.length) {
    const { id: fileId } = uploadFileStrategy.getOrImportSubject(
      adminService,
      releaseId,
    );

    adminService.waitForDataFileToImport({ releaseId, fileId });
  }

  const { subjects } = adminService.getSubjects({ releaseId });
  const subjectId = subjects[0].id;

  return {
    themeId,
    publicationId,
    releaseId,
    subjectId,
  };
}

export function setup(): SetupData {
  const adminService = createAdminService(adminUrl, authTokens.accessToken);

  const { themeId, releaseId, subjectId } = getOrCreateReleaseWithSubject();

  const { subjectMeta } = adminService.getSubjectMeta({ releaseId, subjectId });

  loggingUtils.logDashboardUrls();

  return {
    themeId,
    releaseId,
    subjectId,
    subjectMeta,
  };
}

const performTest = ({ releaseId, subjectId, subjectMeta }: SetupData) => {
  const accessToken = getOrRefreshAccessTokens(userName, authTokens);

  const adminService = createAdminService(adminUrl, accessToken);

  const query = createTableBuilderQuery({
    subjectId,
    subjectMeta,
  });

  const startTimeMillis = Date.now();

  const { response, results } = adminService.tableQuery({
    releaseId,
    query,
  });

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
  }
};

export const teardown = ({ themeId }: SetupData) => {
  if (tearDownData) {
    const accessToken = getOrRefreshAccessTokens(userName, authTokens);

    const adminService = createAdminService(adminUrl, accessToken);

    adminService.deleteTheme({ themeId });

    console.log(`Deleted Theme ${themeId}`);
  }
};

export function handleSummary(data: unknown) {
  return {
    'adminTableBuilderQuery.html': htmlReport(data),
  };
}

export default performTest;
