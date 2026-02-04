/* eslint-disable no-console */
import { check } from 'k6';
import exec from 'k6/execution';
import { Counter, Trend } from 'k6/metrics';
import { Options } from 'k6/options';
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
  subjectId: string;
  subjectMeta: SubjectMeta;
}

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
const { adminUrl, dataApiUrl } = environmentAndUsers.environment;

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

  const { id: releaseVersionId, approvalStatus } =
    adminService.getOrCreateRelease({
      publicationId,
      publicationTitle,
      themeId,
      year: 2000,
      timePeriodCoverage: 'AY',
    });

  const { subjects: existingSubjects } = adminService.getSubjects({
    releaseVersionId,
  });

  if (!existingSubjects.length) {
    console.log('Importing data file');

    const { id: fileId } = uploadFileStrategy.getOrImportSubject(
      adminService,
      releaseVersionId,
    );

    console.log('Waiting for data file to import');

    adminService.waitForDataFileToImport({
      releaseVersionId,
      fileId,
    });

    console.log('Data file imported successfully');
  }

  const { subjects } = adminService.getSubjects({ releaseVersionId });
  const subjectId = subjects[0].id;

  if (approvalStatus !== 'Approved') {
    adminService.addDataGuidance({
      releaseVersionId,
      subjects: [
        {
          id: subjectId,
          content: '<p>Test</p>',
        },
      ],
    });

    console.log('Approving Release');

    adminService.approveRelease({ releaseVersionId });

    console.log(`Waiting for Release to be published`);

    adminService.waitForReleaseToBePublished({ releaseVersionId });

    console.log(`Release published successfully`);
  }

  return {
    themeId,
    releaseVersionId,
    subjectId,
  };
}

export function setup(): SetupData {
  const adminService = createAdminService(adminUrl, authTokens.accessToken);

  const { themeId, releaseVersionId, subjectId } =
    getOrCreateReleaseWithSubject();

  const { subjectMeta } = adminService.getSubjectMeta({
    releaseVersionId,
    subjectId,
  });

  loggingUtils.logDashboardUrls();

  return {
    themeId,
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

export const teardown = ({ themeId }: SetupData) => {
  if (tearDownData) {
    const accessToken = getOrRefreshAccessTokens(userName, authTokens);

    const adminService = createAdminService(adminUrl, accessToken);

    adminService.deleteTheme({ themeId });

    console.log(`Deleted Theme ${themeId}`);
  }
};

export default performTest;
