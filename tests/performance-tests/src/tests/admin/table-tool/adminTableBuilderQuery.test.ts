/* eslint-disable no-restricted-globals */

import { check } from 'k6';
import { Counter, Rate, Trend } from 'k6/metrics';
import { Options } from 'k6/options';
import { AuthDetails } from '../../../auth/getAuthDetails';
import createAdminService, { SubjectMeta } from '../../../utils/adminService';
import getEnvironmentAndUsersFromFile from '../../../utils/environmentAndUsers';
import getOrRefreshAccessTokens from '../../../utils/getOrRefreshAccessTokens';
import logDashboardUrls from '../../../utils/logDashboardUrls';
import logger from '../../../utils/logger';
import utils from '../../../utils/utils';
import testData from '../../testData';

const tearDownData = false;
const publicationTitle =
  __ENV.PUBLICATION_TITLE ?? 'adminTableBuilderQuery.test.ts';
const dataFile = __ENV.DATA_FILE ?? 'small-file.csv';
const uploadFileStrategy = utils.getDataFileUploadStrategy({
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
  linger: true,
  setupTimeout: uploadFileStrategy.isZip ? '30m' : '10m',
};

interface SetupData {
  themeId: string;
  topicId: string;
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
const { adminUrl, supportsRefreshTokens } = environmentAndUsers.environment;

const { authTokens, userName } = environmentAndUsers.users.find(
  user => user.userName === 'bau1',
) as AuthDetails;

function getOrCreateReleaseWithSubject() {
  const adminService = createAdminService(adminUrl, authTokens?.accessToken);

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

  const { id: releaseId } = adminService.getOrCreateRelease({
    publicationId,
    publicationTitle,
    topicId,
    year: 2022,
    timePeriodCoverage: 'AY',
  });

  const { id: fileId } = uploadFileStrategy.getOrImportSubject(
    adminService,
    releaseId,
  );

  adminService.waitForDataFileToImport({ releaseId, fileId });

  const { subjects } = adminService.getSubjects({ releaseId });
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
  const adminService = createAdminService(adminUrl, authTokens.accessToken);

  const {
    themeId,
    topicId,
    releaseId,
    subjectId,
  } = getOrCreateReleaseWithSubject();

  const { subjectMeta } = adminService.getSubjectMeta({ releaseId, subjectId });

  logDashboardUrls();

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

  const adminService = createAdminService(adminUrl, accessToken);

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

  const allLocationIds = Object.values(subjectMeta.locations).flatMap(
    geographicLevel =>
      geographicLevel.options.flatMap(location => {
        if (location.options) {
          return location.options.flatMap(o => o.id);
        }
        return [location.id];
      }),
  );

  const someTimePeriods = {
    startYear: subjectMeta.timePeriod.options[0].year,
    startCode: subjectMeta.timePeriod.options[0].code,
    endYear: subjectMeta.timePeriod.options[1].year,
    endCode: subjectMeta.timePeriod.options[1].code,
  };

  const startTimeMillis = Date.now();

  const { response, results } = adminService.tableQuery({
    releaseId,
    subjectId,
    filterIds: allFilterIds,
    indicatorIds: allIndicationIds,
    locationIds: allLocationIds as string[],
    ...someTimePeriods,
  });

  if (
    check(response, {
      'response code is 200': res => res.status === 200,
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

    logger.info(`Deleted Theme ${themeId}, Topic ${topicId}`);
  }
};

export default performTest;
