/* eslint-disable no-restricted-globals */
import { AuthDetails } from '../auth/getAuthTokens';
import { Environment } from '../auth/storeAuthTokens';

export interface EnvironmentAndUsers {
  environment: Environment;
  users: AuthDetails[];
}

export default function getEnvironmentAndUsersFromFile(
  environmentName: string,
): EnvironmentAndUsers {
  const environmentFilePath = `.env.${environmentName}.json`;
  const environment = JSON.parse(open(environmentFilePath))
    .environment as Environment;

  let loginCredentials: AuthDetails[];

  try {
    const loginCredentialsPath = `.auth-tokens.${environmentName}.json`;
    loginCredentials = JSON.parse(open(loginCredentialsPath)) as AuthDetails[];
  } catch {
    loginCredentials = [];
  }

  return {
    environment,
    users: loginCredentials,
  };
}
