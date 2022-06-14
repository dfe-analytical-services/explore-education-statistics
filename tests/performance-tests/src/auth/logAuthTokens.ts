#!/usr/bin/env node

import getAuthTokens from './getAuthTokens';

interface User {
  name: string;
  email: string;
  password: string;
}

interface Environment {
  baseUrl: string;
  users: User[];
}

const logAuthTokens = async (
  baseUrl: string,
  email: string,
  password: string,
) => {
  const authTokens = await getAuthTokens({
    email,
    password,
    baseUrl,
  });

  /* eslint-disable no-console */
  console.log(JSON.stringify(authTokens));
};

const [environment, userName] = process.argv.slice(2, 4);

/* eslint-disable-next-line @typescript-eslint/no-var-requires */
require('dotenv-json-complex')({ environment });

const env = JSON.parse(process.env.environment as string) as Environment;
const { email, password } = env.users.find(u => u.name === userName) as User;

logAuthTokens(env.baseUrl, email, password);
