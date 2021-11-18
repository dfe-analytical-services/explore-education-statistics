/* eslint-disable no-await-in-loop */
/* eslint-disable no-console */
/* eslint-disable no-case-declarations */
import inquirer from 'inquirer';
import chalk from 'chalk';
import 'dotenv-safe/config';
import uploadSingleSubject from './modules/subject/uploadSubject';
import createPublicationAndRelease from './modules/release/createPublicationAndRelease';
import createReleaseAndPublish from './modules/publication/publishPublication';
import errorHandler from './utils/errorHandler';
import createMethodology from './modules/methodology/createMethodology';
import addMethodlogyTextContentBlock from './modules/methodology/addMethodlogyTextContentBlock';
import createPublication from './modules/publication/createPublication';
import addReleaseTextContentBlock from './modules/release/addContentBlock';
import themeService from './services/themeService';
import topicService from './services/topicService';

process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

const choices = [
  'delete theme & topic',
  'create new publication',
  'create new release',
  'publish a new release',
  'upload subject',
  'create new methodology',
  'generate lorem ipsum content text block (methodology & release)',
] as const;

const start = async () => {
  const answers = await inquirer.prompt({
    name: 'test',
    type: 'list',
    message: 'What test do you want to run?',
    choices,
    prefix: '>',
  });
  try {
    switch (answers.test) {
      case 'create new release':
        await createPublicationAndRelease();
        break;

      case 'delete theme & topic':
        const topic = await inquirer.prompt({
          name: 'id',
          type: 'input',
          message: 'Topic ID',
          prefix: '>',
        });
        const theme = await inquirer.prompt({
          name: 'id',
          type: 'input',
          message: 'Theme ID',
          prefix: '>',
        });
        await topicService.renameTopic(topic.id, theme.id);
        await themeService.renameTheme(theme.id);
        await themeService.deleteTheme(theme.id);
        break;

      case 'create new publication':
        await createPublication();
        break;

      case 'publish a new release':
        await createReleaseAndPublish();
        break;

      case 'upload subject':
        const release = await inquirer.prompt({
          name: 'id',
          type: 'input',
          message: 'Release ID from existing publication',
          prefix: '>',
        });

        const numOfSubjects = await inquirer.prompt({
          name: 'number',
          type: 'number',
          message: 'How many subjects do you want to upload?',
          prefix: '>',
          default: 1,
        });

        const fast = await inquirer.prompt({
          name: 'shouldBeFast',
          type: 'confirm',
          message:
            "would you like the uploadSubject function to exit as soon as a 'QUEUED' status is received? (this can be useful for uploading lots of subjects quickly)",
        });

        for (let i = 0; i < numOfSubjects.number; i += 1) {
          await uploadSingleSubject(release.id, fast.shouldBeFast);
        }
        break;

      case 'create new methodology':
        const publication = await inquirer.prompt({
          name: 'id',
          type: 'input',
          message: 'publication ID from existing publication',
          prefix: '>',
        });
        await createMethodology(publication.id);
        break;

      case 'generate lorem ipsum content text block (methodology & release)':
        const methodologyOrRelease = await inquirer.prompt({
          name: 'type',
          type: 'checkbox',
          choices: ['Methodology', 'Release'],
        });

        const times = await inquirer.prompt({
          name: 'number',
          type: 'number',
          message: 'How many content blocks do you want to create?',
          prefix: '>',
          default: 1,
        });

        switch (methodologyOrRelease.type[0]) {
          case 'Methodology':
            // create a methodology content block
            const publicationExists = await inquirer.prompt({
              name: 'exists',
              type: 'confirm',
              message: 'Do you have an existing publication?',
              prefix: '>',
            });

            if (publicationExists.exists) {
              const methodology = await inquirer.prompt({
                name: 'id',
                type: 'input',
                message: 'methodology ID from existing publication',
                prefix: '>',
              });

              for (let i = 0; i < times.number; i += 1) {
                await addMethodlogyTextContentBlock(methodology.id);
              }
            }
            console.log(
              'Creating new publication, release & methodology with lorem ipsum content',
            );
            const { publicationId } = await createPublicationAndRelease();
            const methodologyId = await createMethodology(publicationId);
            for (let i = 0; i < times.number; i += 1) {
              await addMethodlogyTextContentBlock(methodologyId);
            }
            break;

          case 'Release':
            const releaseExists = await inquirer.prompt({
              name: 'exists',
              type: 'confirm',
              message: 'Do you have an existing release?',
              prefix: '>',
            });

            if (!releaseExists.exists) {
              const { releaseId } = await createPublicationAndRelease();

              for (let i = 0; i < times.number; i += 1) {
                await addReleaseTextContentBlock(releaseId);
              }
            } else {
              const existingRelease = await inquirer.prompt({
                name: 'id',
                type: 'input',
                message: 'release ID from existing release',
                prefix: '>',
              });
              for (let i = 0; i < times.number; i += 1) {
                await addReleaseTextContentBlock(existingRelease.id);
              }
            }
            break;

          default:
            console.error(
              chalk.red('Invalid action:', methodologyOrRelease.type[0]),
            );
        }
        break;

      default:
        console.error(chalk.red('Invalid action:', answers.test));
    }
  } catch (e) {
    errorHandler(e);
  }
};
start();
