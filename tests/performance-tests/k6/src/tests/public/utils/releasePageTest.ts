import http, { RefinedResponse, ResponseType } from 'k6/http';
import { Options } from 'k6/options';
import exec from 'k6/execution';
import grafanaService from '../../../utils/grafanaService';
import { stringifyWithoutNulls } from '../../../utils/utils';
import getEnvironmentAndUsersFromFile from '../../../utils/environmentAndUsers';
import loggingUtils from '../../../utils/loggingUtils';

export interface ReleasePageSetupData {
  buildId: string;
  response: RefinedResponse<ResponseType | undefined>;
}

const environmentAndUsers = getEnvironmentAndUsersFromFile(
  __ENV.TEST_ENVIRONMENT,
);

export default function setupReleasePageTest(
  releasePageUrl: string,
  name: string,
): ReleasePageSetupData {
  loggingUtils.logDashboardUrls();

  const response = http.get(
    `${environmentAndUsers.environment.publicUrl}${releasePageUrl}?redesign=true`,
  );
  const regexp = /"buildId":"([-0-9a-zA-Z]*)"/g;
  const buildId = regexp.exec(response.body as string)![1];

  logTestStart(exec.test.options, name);

  grafanaService.testStart({
    name,
    config: exec.test.options,
  });

  return {
    buildId,
    response,
  };
}

function logTestStart(config: Options, name: string) {
  console.log(
    `Starting test ${name}, with configuration:\n\n${stringifyWithoutNulls(
      config,
    )}\n\n`,
  );
}
