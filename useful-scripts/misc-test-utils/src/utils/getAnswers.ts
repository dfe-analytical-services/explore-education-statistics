import chalk from 'chalk';
import { prompt } from 'inquirer';
import promptService from '../services/promptService';
import errorHandler from './errorHandler';
import logger from './logger';

const getAnswers = async (choices: readonly string[]) => {
  const answers = await prompt({
    name: 'test',
    type: 'list',
    message: 'What test do you want to run?',
    choices,
    prefix: '>',
  });
  try {
    switch (answers.test) {
      case 'create new release':
        await promptService.createRelease();
        break;

      case 'delete theme & topic':
        await promptService.deleteThemeAndTopic();
        break;

      case 'create new publication':
        await promptService.createPublication();
        break;

      case 'create new release & publication':
        await promptService.createPublicationAndRelease();
        break;

      case 'publish a new release':
        await promptService.createReleaseAndPublish();
        break;

      case 'publish all releases':
        await promptService.publishAllReleases();
        break;

      case 'upload subject':
        await promptService.uploadSubject();
        break;

      case 'create new methodology':
        await promptService.createMethodology();
        break;

      case 'generate lorem ipsum content text block (methodology & release)':
        await promptService.generateLoremIpsumContentTextBlock();
        break;

      case 'upload many subjects & publish':
        await promptService.uploadManySubjectsAndPublish();
        break;

      default:
        logger.error(chalk.red('Invalid action:', answers.test));
    }
  } catch (e) {
    errorHandler(e);
  }
};
export default getAnswers;
