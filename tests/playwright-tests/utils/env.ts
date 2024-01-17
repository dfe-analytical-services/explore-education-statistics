import dotenv from 'dotenv';

dotenv.config();

const environment = {
  PUBLIC_URL: process.env.PUBLIC_URL ?? '',
  ADMIN_URL: process.env.ADMIN_URL ?? '',
  ADMIN_EMAIL: process.env.ADMIN_EMAIL ?? '',
  ADMIN_PASSWORD: process.env.ADMIN_PASSWORD ?? '',
};

export default environment;
