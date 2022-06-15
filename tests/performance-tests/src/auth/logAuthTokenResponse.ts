#!/usr/bin/env node

import { getAuthTokens } from './getAuthDetails';

interface User {
  name: string;
  email: string;
  password: string;
}

interface Environment {
  adminUrl: string;
  users: User[];
}

const logAuthTokenResponse = async (
  email: string,
  password: string,
  adminUrl: string,
) => {
  const rawTokenResponse = await getAuthTokens(email, password, adminUrl);

  /* eslint-disable-next-line no-console */
  console.log(rawTokenResponse);
};

const [environment, userName] = process.argv.slice(2, 4);

/* eslint-disable-next-line @typescript-eslint/no-var-requires */
require('dotenv-json-complex')({ environment });

const env = JSON.parse(process.env.environment as string) as Environment;
const { email, password } = env.users.find(u => u.name === userName) as User;

logAuthTokenResponse(email, password, env.adminUrl);
