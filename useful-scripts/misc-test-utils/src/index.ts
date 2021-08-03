import inquirer from 'inquirer';
import chalk from 'chalk';
import 'dotenv-safe/config';
import uploadSingleSubject from './modules/subject/uploadSubject';
import createPublicationAndRelease from './modules/release/createRelease';
import createReleaseAndPublish from './modules/publication/publishPublication';

process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

const choices = [
  'create new release',
  'publish a new release',
  'upload subject',
] as const;

const start = async () => {
  const answers = await inquirer.prompt({
    name: 'test',
    type: 'list',
    message: 'What test do you want to run?',
    choices,
    prefix: '>',
  });

  switch (answers.test) {
    case 'create new release':
      await createPublicationAndRelease();
      break;

    case 'publish a new release':
      await createReleaseAndPublish();
      break;

    case 'upload subject':
      // eslint-disable-next-line no-case-declarations
      const release = await inquirer.prompt({
        name: 'id',
        type: 'input',
        message: 'Release ID from existing publication',
        prefix: '>',
      });

      await uploadSingleSubject(release.id);
      break;

    default:
      // eslint-disable-next-line no-console
      console.error(chalk.red('Invalid action:', answers.test));
  }
};
start();
