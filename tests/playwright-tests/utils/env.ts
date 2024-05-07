import dotenv from 'dotenv';
import generateRunIdentifier from './generateRunIdentifier';

dotenv.config();

const environment = {
  PUBLIC_URL: process.env.PUBLIC_URL ?? '',
  PUBLIC_USERNAME: process.env.PUBLIC_USERNAME ?? '',
  PUBLIC_PASSWORD: process.env.PUBLIC_PASSWORD ?? '',
  ADMIN_URL: process.env.ADMIN_URL ?? '',
  ADMIN_EMAIL: process.env.ADMIN_EMAIL ?? '',
  ADMIN_PASSWORD: process.env.ADMIN_PASSWORD ?? '',
  RUN_IDENTIFIER: generateRunIdentifier(),
};

export default environment;
