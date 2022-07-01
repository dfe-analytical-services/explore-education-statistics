import { check } from 'k6';
import { Counter, Rate, Trend } from 'k6/metrics';
import { Options } from 'k6/options';
import { AuthDetails, AuthTokens } from '../../auth/getAuthDetails';
import createDataService, { SubjectMeta } from '../../utils/dataService';
import testData from '../testData';
import getOrRefreshAccessTokens from '../../utils/getOrRefreshAccessTokens';

const PUBLICATION =
  'UI test publication - Performance tests - adminTableBuilderQuery.test.ts';
const RELEASE = '2022';
const SUBJECT =
  'UI test subject - Performance tests - adminTableBuilderQuery.test.ts';

const alwaysCreateNewDataPerTest = false;

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
  linger: true,
  setupTimeout: '5m',
};

interface SetupData {
  themeId: string;
  topicId: string;
  releaseId: string;
  subjectId: string;
  subjectMeta: SubjectMeta;
  adminUrl: string;
  userName: string;
  authTokens: AuthTokens;
  supportsRefreshTokens: boolean;
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
const subjectFile = open('import/assets/dates.csv', 'b');
const subjectMetaFile = open('import/assets/dates.meta.csv', 'b');
/* eslint-enable no-restricted-globals */

function getOrCreateReleaseWithSubject(adminUrl: string, accessToken: string) {
  const dataService = createDataService(adminUrl, accessToken);

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
    releaseName: RELEASE,
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

  return {
    themeId,
    topicId,
    publicationId,
    releaseId,
    subjectId,
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

  const dataService = createDataService(adminUrl, authTokens.accessToken);

  const {
    themeId,
    topicId,
    releaseId,
    subjectId,
  } = getOrCreateReleaseWithSubject(adminUrl, authTokens.accessToken);

  const { subjectMeta } = dataService.getSubjectMeta({ releaseId, subjectId });

  return {
    themeId,
    topicId,
    releaseId,
    subjectId,
    subjectMeta,
    userName,
    adminUrl,
    authTokens,
    supportsRefreshTokens,
  };
}

const performTest = ({
  releaseId,
  subjectId,
  userName,
  adminUrl,
  authTokens,
  subjectMeta,
  supportsRefreshTokens,
}: SetupData) => {
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
