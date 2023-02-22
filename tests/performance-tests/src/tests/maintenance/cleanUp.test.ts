import { Options } from 'k6/options';
import { AuthDetails } from '../../auth/getAuthDetails';
import createAdminService from '../../utils/adminService';
import getEnvironmentAndUsersFromFile from '../../utils/environmentAndUsers';
import getOrRefreshAccessTokens from '../../utils/getOrRefreshAccessTokens';
import logger from '../../utils/logger';
import testData from '../testData';

export const options: Options = {
  insecureSkipTLSVerify: true,
  teardownTimeout: '120s',
};

const environmentAndUsers = getEnvironmentAndUsersFromFile(
  __ENV.TEST_ENVIRONMENT,
);

const performTest = () => {
  return true;
};

export const teardown = () => {
  const { adminUrl, supportsRefreshTokens } = environmentAndUsers.environment;

  const { authTokens, userName } = environmentAndUsers.users.find(
    user => user.userName === 'bau1',
  ) as AuthDetails;

  const accessToken = getOrRefreshAccessTokens(
    supportsRefreshTokens,
    userName,
    adminUrl,
    authTokens,
  );

  const adminService = createAdminService(adminUrl, accessToken);

  const testTheme = adminService.getTheme({ title: testData.themeName });

  if (testTheme) {
    const testTopic = adminService.getTopic({
      themeId: testTheme?.id,
      title: testData.topicName,
    });
    if (testTopic) {
      adminService.deleteTopic({ topicId: testTopic.id });
      logger.info(`Deleted Topic ${testData.topicName}`);
    }
    adminService.deleteTheme({ themeId: testTheme.id });
    logger.info(`Deleted Theme ${testData.themeName}`);
  }
};

export default performTest;
