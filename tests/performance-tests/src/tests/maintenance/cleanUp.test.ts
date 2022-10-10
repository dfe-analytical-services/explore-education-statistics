/* eslint-disable no-console */
import createAdminService from '../../utils/adminService';
import getOrRefreshAccessTokens from '../../utils/getOrRefreshAccessTokens';
import getEnvironmentAndUsersFromFile from '../../utils/environmentAndUsers';
import testData from '../testData';

const performTest = () => {};

export const teardown = () => {
  const environmentAndUsers = getEnvironmentAndUsersFromFile(
    __ENV.TEST_ENVIRONMENT as string,
  );
  const { adminUrl, supportsRefreshTokens } = environmentAndUsers.environment;

  // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
  const { authTokens, userName } = environmentAndUsers.users.find(
    user => user.userName === 'bau1',
  )!;

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
      console.log(`Deleted Topic ${testData.topicName}`);
    }
    adminService.deleteTheme({ themeId: testTheme.id });
    console.log(`Deleted Theme ${testData.themeName}`);
  }
};

export default performTest;
