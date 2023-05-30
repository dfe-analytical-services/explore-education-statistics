#!/usr/bin/env node
import fs from 'fs';
import dotenvJson from 'dotenv-json-complex';
import { EnvironmentAndUsers } from '../utils/environmentAndUsers';
import getAuthDetails, { IdpOption } from './getAuthDetails';

export interface User {
  name: string;
  email: string;
  password: string;
}

export interface Environment {
  publicUrl: string;
  adminUrl: string;
  contentApiUrl: string;
  dataApiUrl: string;
  publicApiUrl: string;
  idp: IdpOption;
  users: User[];
  supportsRefreshTokens: boolean;
}

const getEnvironmentAndUsers = async (
  environmentName: string,
  userNames: string[],
): Promise<EnvironmentAndUsers> => {
  dotenvJson({ environment: environmentName });

  const environment = JSON.parse(
    process.env.environment as string,
  ) as Environment;

  const users = environment.users.filter(u => userNames.includes(u.name));

  const authTokens = await Promise.all(
    users.map(async ({ name, email, password }) => {
      return getAuthDetails(
        name,
        email,
        password,
        environment.adminUrl,
        environment.idp,
      );
    }),
  );

  return {
    environment,
    users: authTokens,
  };
};

const writeEnvironmentDetailsToFile = async (
  environmentName: string,
  userNames: string[],
): Promise<void> => {
  const environmentAndUsers = await getEnvironmentAndUsers(
    environmentName,
    userNames,
  );
  const filepath = `/home/node/app/dist/.environment-details.${environmentName}.json`;
  fs.writeFileSync(filepath, JSON.stringify(environmentAndUsers));
};

const [environment, ...userNames] = process.argv.slice(2);
writeEnvironmentDetailsToFile(environment, userNames);
