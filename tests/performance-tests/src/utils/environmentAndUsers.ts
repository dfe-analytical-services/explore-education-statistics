/* eslint-disable no-restricted-globals */
import { AuthDetails } from '../auth/getAuthDetails';
import { Environment } from '../auth/storeEnvironmentDetails';

export interface EnvironmentAndUsers {
  environment: Environment;
  users: AuthDetails[];
}

export default function getEnvironmentAndUsersFromFile(
  environmentName: string,
): EnvironmentAndUsers {
  const environmentFilePath = `.env.${environmentName}.json`;
  const environment = JSON.parse(open(environmentFilePath)) as Environment;

  const loginCredentialsPath = `.login-tokens.${environmentName}.json`;
  const loginCredentials = JSON.parse(
    open(loginCredentialsPath),
  ) as AuthDetails[];

  return {
    environment,
    users: loginCredentials,
  };
}
