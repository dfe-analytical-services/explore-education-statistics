/* eslint-disable no-console */
import { AxiosError } from 'axios';
import chalk from 'chalk';
import logger from './logger';

const errorHandler = (e: AxiosError): void => {
  if (!e.isAxiosError) {
    logger.error(e);
    process.exit(1);
  }

  switch (e.response?.status) {
    case 401:
      logger.error(
        chalk.red`JWT token has expired, get a new one from Admin EES`,
      );
      break;

    case 400:
      logger.error(
        chalk.red`Bad request made to server, check your code is correct ${e}`,
      );
      logger.error(e.response.data);
      break;

    case 500:
      logger.error(
        chalk.red`something is not working. Check everything is ok with the environment you're testing against ${e}`,
      );
      logger.error(e.response.data);
      break;

    case 503:
      logger.error(
        chalk.red`something is not working. Check everything is ok with the environment you're testing against ${e}`,
      );
      logger.error(e.response.data);
      break;

    default:
      logger.error(e);
      break;
  }

  process.exit(1);
};

export default errorHandler;
