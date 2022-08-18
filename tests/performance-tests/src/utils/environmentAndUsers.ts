import { AuthDetails } from '../auth/getAuthDetails';
import { Environment } from '../auth/storeEnvironmentDetails';

export interface EnvironmentAndUsers {
  environment: Environment;
  users: AuthDetails[];
}

export default function getEnvironmentAndUsersFromFile(
  environmentName: string,
): EnvironmentAndUsers {
  const environmentAndUsersFilePath = `.environment-details.${environmentName}.json`;
  /* eslint-disable-next-line no-restricted-globals */
  return JSON.parse(open(environmentAndUsersFilePath)) as EnvironmentAndUsers;
}
