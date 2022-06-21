#!/usr/bin/env node

import getAuthDetails, { IdpOption } from './getAuthDetails';

export interface User {
  name: string;
  email: string;
  password: string;
}

export interface Environment {
  adminUrl: string;
  idp: IdpOption;
  users: User[];
}

const logAuthDetails = async (users: User[]) => {
  const authTokens = await Promise.all(
    users.map(async ({ name, email, password }) => {
      return getAuthDetails(name, email, password, env.adminUrl, env.idp);
    }),
  );

  /* eslint-disable-next-line no-console */
  console.log(JSON.stringify(authTokens));
};

const [environment, ...userNames] = process.argv.slice(2);

/* eslint-disable-next-line @typescript-eslint/no-var-requires */
require('dotenv-json-complex')({ environment });

const env = JSON.parse(process.env.environment as string) as Environment;

const userDetails = userNames.length
  ? env.users.filter(u => userNames.includes(u.name))
  : env.users;

logAuthDetails(userDetails);
