/* eslint-disable no-console */
import { check } from 'k6';
import { Counter, Rate, Trend } from 'k6/metrics';
import { Options } from 'k6/options';
import createAdminService, { SubjectMeta } from '../../../utils/adminService';
import testData from '../../testData';
import getOrRefreshAccessTokens from '../../../utils/getOrRefreshAccessTokens';
import getEnvironmentAndUsersFromFile from '../../../utils/environmentAndUsers';
import createDataService from '../../../utils/dataService';
import constants from '../../../utils/constants';

const PUBLICATION =
  'UI test publication - Performance tests - publicTableBuilderQuery.test.ts';
const RELEASE = 2022;
const SUBJECT =
  'UI test subject - Performance tests - publicTableBuilderQuery.test.ts';

const alwaysCreateNewDataPerTest = false;
const bigFile = true;

export const options: Options = {
  scenarios: {
    constant_request_rate: {
      executor: 'constant-arrival-rate',
      rate: 1,
      timeUnit: '5s',
      duration: '120m',
      preAllocatedVUs: 1,
      maxVUs: 1,
    },
  },
  noConnectionReuse: true,
  insecureSkipTLSVerify: true,
  linger: true,
  setupTimeout: bigFile ? '30m' : '10m',
};

interface SetupData {
  themeId: string;
  topicId: string;
  publicationId: string;
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

/* eslint-disable no-restricted-globals */
const zipFile = bigFile ? open('admin/import/assets/big-file1.zip', 'b') : null;
const subjectFile = !bigFile
  ? open('admin/import/assets/dates.csv', 'b')
  : null;
const subjectMetaFile = !bigFile
  ? open('admin/import/assets/dates.meta.csv', 'b')
  : null;
/* eslint-enable no-restricted-globals */

const environmentAndUsers = getEnvironmentAndUsersFromFile(
  __ENV.TEST_ENVIRONMENT as string,
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

  const suffix = alwaysCreateNewDataPerTest
    ? `-${Date.now()}-${Math.random()}`
    : '';

  const { id: themeId } = adminService.getOrCreateTheme({
    title: `${testData.themeName}${suffix}`,
  });

  const { id: topicId } = adminService.getOrCreateTopic({
    themeId,
    title: `${testData.topicName}${suffix}`,
  });

  const publicationTitle = `${PUBLICATION}${suffix}`;

  const { id: publicationId } = adminService.getOrCreatePublication({
    topicId,
    title: publicationTitle,
  });

  const { id: releaseId, approvalStatus } = adminService.getOrCreateRelease({
    publicationId,
    publicationTitle,
    topicId,
    year: RELEASE,
    timePeriodCoverage: 'AY',
  });

  const { subjects: existingSubjects } = adminService.getSubjects({
    releaseId,
  });

  if (!existingSubjects.length) {
    console.log('Importing data file');

    const { id: fileId } = bigFile
      ? adminService.getOrImportDataZipFile({
          title: `${SUBJECT}${suffix}`,
          releaseId,
          zipFile: {
            file: zipFile!,
            filename: `subject.zip`,
          },
        })
      : adminService.getOrImportDataFile({
          title: `${SUBJECT}${suffix}`,
          releaseId,
          dataFile: {
            file: subjectFile!,
            filename: `subject.csv`,
          },
          metaFile: {
            file: subjectMetaFile!,
            filename: `subject.meta.csv`,
          },
        });

    console.log('Waiting for data file to import');

    adminService.waitForDataFileToImport({
      releaseId,
      fileId,
    });
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

    console.log(`Approving Release ${RELEASE}`);

    adminService.approveRelease({ releaseId });

    console.log(`Waiting for Release ${RELEASE} to be published`);

    adminService.waitForReleaseToBePublished({ releaseId });

    console.log(`Release ${RELEASE} published successfully`);
  }

  return {
    themeId,
    topicId,
    publicationId,
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
    publicationId,
    subjectId,
  } = getOrCreateReleaseWithSubject();

  const { subjectMeta } = adminService.getSubjectMeta({ releaseId, subjectId });

  console.log(
    `\n\nEES performance results available at: ${constants.grafanaEesDashboardUrl}`,
  );
  console.log(
    `generic performance results available at: ${constants.grafanaGenericDashboardUrl}\n\n`,
  );

  return {
    themeId,
    topicId,
    publicationId,
    subjectId,
    subjectMeta,
  };
}

const performTest = ({ publicationId, subjectId, subjectMeta }: SetupData) => {
  const dataService = createDataService(dataApiUrl);

  const allFilterIds = Object.values(subjectMeta.filters).flatMap(filter =>
    Object.values(filter.options).flatMap(filterGroup =>
      filterGroup.options.flatMap(filterItem => filterItem.value),
    ),
  );

  const allIndicationIds = Object.values(
    subjectMeta.indicators,
  ).flatMap(indicatorGroup =>
    indicatorGroup.options.map(indicator => indicator.value),
  );

  const allLocationIds = Object.values(
    subjectMeta.locations,
  ).flatMap(geographicLevel =>
    geographicLevel.options.map(location => location.id),
  );

  const someTimePeriods = {
    startYear: subjectMeta.timePeriod.options[0].year,
    startCode: subjectMeta.timePeriod.options[0].code,
    endYear: subjectMeta.timePeriod.options[1].year,
    endCode: subjectMeta.timePeriod.options[1].code,
  };

  const startTimeMillis = Date.now();

  const { response, results } = dataService.tableQuery({
    publicationId,
    subjectId,
    filterIds: allFilterIds,
    indicatorIds: allIndicationIds,
    locationIds: allLocationIds,
    ...someTimePeriods,
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

    console.log(`Table query completed in ${tableQuerySpeed / 1000} seconds`);
  } else {
    tableQueryFailureCount.add(1);
    console.log(`Table query failed`);
  }
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
