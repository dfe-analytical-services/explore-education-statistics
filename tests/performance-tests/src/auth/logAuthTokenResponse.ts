#!/usr/bin/env node

import { getAuthTokens, IdpOption } from './getAuthDetails';
import { Environment, User } from './logAuthDetails';

const logAuthTokenResponse = async (
  email: string,
  password: string,
  adminUrl: string,
  idp: IdpOption,
) => {
  const rawTokenResponse = await getAuthTokens(email, password, adminUrl, idp);

  /* eslint-disable-next-line no-console */
  console.log(rawTokenResponse);
};

const [environment, userName] = process.argv.slice(2, 4);

/* eslint-disable-next-line @typescript-eslint/no-var-requires */
require('dotenv-json-complex')({ environment });

const env = JSON.parse(process.env.environment as string) as Environment;
const { email, password } = env.users.find(u => u.name === userName) as User;

logAuthTokenResponse(email, password, env.adminUrl, env.idp);
