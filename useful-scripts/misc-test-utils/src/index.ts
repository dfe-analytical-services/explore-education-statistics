/* eslint-disable */
import inquirer from 'inquirer';
import createReleaseAndPublish from './modules/publication/publish';
import createSingleRelease from './modules/release/create';
import uploadSubect from './modules/subject/upload';
import chalk from 'chalk';

type choiceT =
  | 'create new release'
  | 'publish a new release'
  | 'upload subject';

const choices: choiceT[] = [
  'create new release',
  'publish a new release',
  'upload subject',
];

const start = () => {
  inquirer
    .prompt({
      name: 'test you want to run',
      type: 'list',
      message: 'What test do you want to run?',
      choices,
      prefix: '>',
    })
    .then(async answer => {
      switch (answer['test you want to run']) {
        case 'create new release':
          await createSingleRelease();
          break;

        case 'publish a new release':
          await createReleaseAndPublish();
          break;

        case 'upload subject':
          const releaseId = await inquirer.prompt({
            name: 'release id',
            type: 'input',
            prefix: '>',
          });
          const releaseIdString = releaseId['release id'];
          await uploadSubect(releaseIdString);
          break;

        default:
          console.log(
            chalk.red('Invalid action:', answer['test you want to run']),
          );
      }
    })
    .catch(e => {
      console.log(e);
    });
};
start();
