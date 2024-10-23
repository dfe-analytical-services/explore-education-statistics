#!/usr/bin/env node
import fs from 'fs';
import dotenvJson from 'dotenv-json-complex';
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
  users: User[];
  idp: IdpOption;
  openIdConnect: {
    clientId: string;
    refreshTokenUrl: string;
  };
}

const writeLoginCredentialsToFile = async (
  environmentName: string,
  userNames: string[],
): Promise<void> => {
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

  const loginCredentialsFilePath = `/home/node/app/dist/.login-tokens.${environmentName}.json`;
  fs.writeFileSync(loginCredentialsFilePath, JSON.stringify(authTokens));
};

const [environment, ...userNames] = process.argv.slice(2);
writeLoginCredentialsToFile(environment, userNames);
