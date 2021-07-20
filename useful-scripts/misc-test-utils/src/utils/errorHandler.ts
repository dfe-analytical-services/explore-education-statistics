/* eslint-disable no-console */
import { AxiosError } from 'axios';
import chalk from 'chalk';

const errorHandler = (e: AxiosError): void => {
  if (!e.isAxiosError) {
    console.error(e);
    process.exit(1);
  }

  switch (e.response?.status) {
    case 401:
      console.log(
        chalk.red`JWT token has expired, get a new one from Admin EES`,
      );
      break;

    case 400:
      console.log(
        chalk.red`Bad request made to server, check your code is correct ${e}`,
      );
      console.error(e.response.data);
      break;

    case 500:
      console.log(
        chalk.red`something is not working. Check everything is ok with the environment you're testing against ${e}`,
      );
      console.error(e.response.data);
      break;

    case 503:
      console.log(
        chalk.red`something is not working. Check everything is ok with the environment you're testing against ${e}`,
      );
      console.error(e.response.data);
      break;

    default:
      console.error(e);
      break;
  }

  process.exit(1);
};

export default errorHandler;
