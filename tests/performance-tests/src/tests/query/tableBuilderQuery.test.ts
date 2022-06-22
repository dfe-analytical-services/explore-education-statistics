import { check, sleep } from 'k6';
import { Counter, Rate, Trend } from 'k6/metrics';
import { Options } from 'k6/options';
import refreshAuthTokens from '../../auth/refreshAuthTokens';
import { AuthDetails, AuthTokens } from '../../auth/getAuthDetails';
import createDataService, { SubjectMeta } from '../../utils/dataService';

export const options: Options = {
  scenarios: {
    constant_request_rate: {
      executor: 'constant-arrival-rate',
      rate: 1,
      timeUnit: '0.5s',
      duration: '100m',
      preAllocatedVUs: 20,
      maxVUs: 100,
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
    title: `UI test theme - Performance tests - "tableBuilderQuery.test.ts" - ${uniqueId}`,
  });

  const { topicId } = dataService.createTopic({
    themeId,
    title: `UI test topic - Performance tests - "tableBuilderQuery.test.ts" - ${uniqueId}`,
  });

  const { publicationId } = dataService.createPublication({
    topicId,
    title: `UI test publication - Performance tests - "tableBuilderQuery.test.ts" - ${uniqueId}`,
  });

  const { releaseId } = dataService.createRelease({
    publicationId,
    releaseName: '2022',
    timePeriodCoverage: 'AY',
  });

  const { fileId } = dataService.importDataFile({
    title: 'Data file for querying',
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

  const maxImportWaitTimeMillis = 240 * 1000;
  const importStartTime = Date.now();
  const importExpireTime = importStartTime + maxImportWaitTimeMillis;

  while (Date.now() < importExpireTime) {
    sleep(5);

    const { importStatus } = dataService.getImportStatus({
      releaseId,
      fileId,
    });

    if (importStatus === 'FAILED' || importStatus === 'CANCELLED') {
      errorRate.add(1);
      throw new Error(
        `Incorrect end state for import process of uploaded subject file - ${importStatus}`,
      );
    }

    if (importStatus === 'COMPLETE') {
      break;
    }
  }

  const { subjects } = dataService.getSubjects({ releaseId });
  const subjectId = subjects[0].id;
  const { subjectMeta } = dataService.getSubjectMeta({ releaseId, subjectId });

  /* eslint-disable-next-line no-console */
  console.log(
    `Created Theme ${themeId}, Topic ${topicId}, Publication ${publicationId}, Release ${releaseId}`,
  );

  /* eslint-disable-next-line no-console */
  console.log(`Created Subject ${subjectId}`);

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
  subjectId,
  userName,
  adminUrl,
  authTokens,
  subjectMeta,
  supportsRefreshTokens,
}: SetupData) => {
  const accessToken = getOrRefreshAccessToken(
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
