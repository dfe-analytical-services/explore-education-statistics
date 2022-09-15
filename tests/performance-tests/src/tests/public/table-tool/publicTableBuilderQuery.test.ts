/* eslint-disable no-console */
import { check } from 'k6';
import { Counter, Rate, Trend } from 'k6/metrics';
import { Options } from 'k6/options';
import createDataService, { SubjectMeta } from '../../../utils/dataService';
import testData from '../../testData';
import getOrRefreshAccessTokens from '../../../utils/getOrRefreshAccessTokens';
import getEnvironmentAndUsersFromFile from '../../../utils/environmentAndUsers';

const PUBLICATION =
  'UI test publication - Performance tests - publicTableBuilderQuery.test.ts';
const RELEASE = 2022;
const SUBJECT =
  'UI test subject - Performance tests - publicTableBuilderQuery.test.ts';

const alwaysCreateNewDataPerTest = false;

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
  setupTimeout: '10m',
};

interface SetupData {
  themeId: string;
  topicId: string;
  releaseId: string;
  subjectId: string;
  subjectMeta: SubjectMeta;
}

export const errorRate = new Rate('ees_errors');
export const tableQuerySpeedTrend = new Trend('ees_table_query_speed', true);
export const tableQueryCompleteCount = new Counter(
  'ees_table_query_complete_count',
);
export const tableQueryFailureCount = new Counter(
  'ees_table_query_failure_count',
);

/* eslint-disable no-restricted-globals */
const subjectFile = open('admin/import/assets/dates.csv', 'b');
const subjectMetaFile = open('admin/import/assets/dates.meta.csv', 'b');
/* eslint-enable no-restricted-globals */

const environmentAndUsers = getEnvironmentAndUsersFromFile(
  __ENV.TEST_ENVIRONMENT as string,
);
const { adminUrl, supportsRefreshTokens } = environmentAndUsers.environment;

// eslint-disable-next-line @typescript-eslint/no-non-null-assertion
const { authTokens, userName } = environmentAndUsers.users.find(
  user => user.userName === 'bau1',
)!;

function getOrCreateReleaseWithSubject() {
  const dataService = createDataService(
    adminUrl,
    authTokens?.accessToken as string,
  );

  const suffix = alwaysCreateNewDataPerTest
    ? `-${Date.now()}-${Math.random()}`
    : '';

  const { id: themeId } = dataService.getOrCreateTheme({
    title: `${testData.themeName}${suffix}`,
  });

  const { id: topicId } = dataService.getOrCreateTopic({
    themeId,
    title: `${testData.topicName}${suffix}`,
  });

  const publicationTitle = `${PUBLICATION}${suffix}`;

  const { id: publicationId } = dataService.getOrCreatePublication({
    topicId,
    title: publicationTitle,
  });

  const { id: releaseId } = dataService.getOrCreateRelease({
    publicationId,
    publicationTitle,
    topicId,
    year: RELEASE,
    timePeriodCoverage: 'AY',
  });

  const { id: fileId } = dataService.getOrImportDataFile({
    title: `${SUBJECT}${suffix}`,
    releaseId,
    dataFile: {
      file: subjectFile,
      filename: `subject.csv`,
    },
    metaFile: {
      file: subjectMetaFile,
      filename: `subject.meta.csv`,
    },
  });

  dataService.waitForDataFileToImport({ releaseId, fileId });

  const { subjects } = dataService.getSubjects({ releaseId });
  const subjectId = subjects[0].id;

  dataService.addDataGuidance({
    releaseId,
    subjects: [
      {
        id: subjectId,
        content: '<p>Test</p>',
      },
    ],
  });

  dataService.approveRelease({ releaseId });

  dataService.waitForReleaseToBePublished({ releaseId });

  return {
    themeId,
    topicId,
    publicationId,
    releaseId,
    subjectId,
  };
}

export function setup(): SetupData {
  const dataService = createDataService(adminUrl, authTokens.accessToken);

  const {
    themeId,
    topicId,
    releaseId,
    subjectId,
  } = getOrCreateReleaseWithSubject();

  const { subjectMeta } = dataService.getSubjectMeta({ releaseId, subjectId });

  return {
    themeId,
    topicId,
    releaseId,
    subjectId,
    subjectMeta,
  };
}

const performTest = ({ releaseId, subjectId, subjectMeta }: SetupData) => {
  const accessToken = getOrRefreshAccessTokens(
    supportsRefreshTokens,
    userName,
    adminUrl,
    authTokens,
  );

  const dataService = createDataService(adminUrl, accessToken);

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
    releaseId,
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
    tableQueryCompleteCount.add(1);
    tableQuerySpeedTrend.add(Date.now() - startTimeMillis);
  } else {
    tableQueryFailureCount.add(1);
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

    const dataService = createDataService(adminUrl, accessToken);

    dataService.deleteTopic({ topicId });
    dataService.deleteTheme({ themeId });

    console.log(`Deleted Theme ${themeId}, Topic ${topicId}`);
  }
};

export default performTest;
