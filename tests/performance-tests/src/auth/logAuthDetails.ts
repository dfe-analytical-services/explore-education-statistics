#!/usr/bin/env node

import getAuthDetails from './getAuthDetails';

export interface User {
  userName: string;
  email: string;
  password: string;
}

interface Environment {
  adminUrl: string;
  users: User[];
}

const logAuthDetails = async (users: User[]) => {
  const authTokens = await Promise.all(
    users.map(async ({ userName, email, password }) => {
      return getAuthDetails(userName, email, password, env.adminUrl);
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
  ? env.users.filter(u => userNames.includes(u.userName))
  : env.users;

logAuthDetails(userDetails);
