/* eslint-disable no-case-declarations */
/* eslint-disable no-await-in-loop */
/* eslint-disable no-console */
import { prompt } from 'inquirer';
import chalk from 'chalk';
import Guid from '../utils/Guid';
import createPublicationAndRelease from '../modules/release/createPublicationAndRelease';
import themeService from './themeService';
import topicService from './topicService';
import createPublication from '../modules/publication/createPublication';
import createReleaseAndPublish from '../modules/publication/publishPublication';
import uploadSingleSubject from '../modules/subject/uploadSubject';
import createMethodology from '../modules/methodology/createMethodology';
import addMethodlogyTextContentBlock from '../modules/methodology/addMethodlogyTextContentBlock';
import addReleaseTextContentBlock from '../modules/release/addContentBlock';
import logger from '../utils/logger';
import releaseService from './releaseService';

const promptService = {
  createPublicationAndRelease: async () => {
    await createPublicationAndRelease();
  },

  createRelease: async () => {
    const publication = await prompt({
      name: 'id',
      type: 'input',
      message: 'Enter publication id:',
      prefix: '>',
      validate: async input => {
        if (!Guid.isGuid(input)) {
          return 'Not a valid GUID';
        }
        return true;
      },
    });
    await releaseService.createRelease(publication.id);
  },

  deleteThemeAndTopic: async () => {
    const topic = await prompt({
      name: 'id',
      type: 'input',
      message: 'Topic ID',
      prefix: '>',
      validate: async input => {
        if (!Guid.isGuid(input)) {
          return 'Not a valid GUID';
        }
        return true;
      },
    });
    const theme = await prompt({
      name: 'id',
      type: 'input',
      message: 'Theme ID',
      prefix: '>',
      validate: async input => {
        if (!Guid.isGuid(input)) {
          return 'Not a valid GUID';
        }
        return true;
      },
    });
    await topicService.renameTopic(topic.id, theme.id);
    await themeService.renameTheme(theme.id);
    await themeService.deleteTheme(theme.id);
  },

  createPublication: async () => {
    await createPublication();
  },

  createReleaseAndPublish: async () => {
    await createReleaseAndPublish();
  },

  uploadSubject: async () => {
    const release = await prompt({
      name: 'id',
      type: 'input',
      message: 'Release ID from existing publication',
      prefix: '>',
      validate: async input => {
        if (!Guid.isGuid(input)) {
          return 'Not a valid GUID';
        }
        return true;
      },
    });

    const numOfSubjects = await prompt({
      name: 'number',
      type: 'number',
      message: 'How many subjects do you want to upload?',
      prefix: '>',
      default: 1,
    });

    const fast = await prompt({
      name: 'shouldBeFast',
      type: 'confirm',
      message:
        "would you like the uploadSubject function to exit as soon as a 'QUEUED' status is received? (this can be useful for uploading lots of subjects quickly)",
    });

    for (let i = 0; i < numOfSubjects.number; i += 1) {
      // eslint-disable-next-line no-await-in-loop
      await uploadSingleSubject(release.id, fast.shouldBeFast);
    }
  },

  createMethodology: async () => {
    const publication = await prompt({
      name: 'id',
      type: 'input',
      message: 'publication ID from existing publication',
      prefix: '>',
      validate: async input => {
        if (!Guid.isGuid(input)) {
          return 'Not a valid GUID';
        }
        return true;
      },
    });
    await createMethodology(publication.id);
  },

  generateLoremIpsumContentTextBlock: async () => {
    const methodologyOrRelease = await prompt({
      name: 'type',
      type: 'checkbox',
      choices: ['Methodology', 'Release'],
    });

    const times = await prompt({
      name: 'number',
      type: 'number',
      message: 'How many content blocks do you want to create?',
      prefix: '>',
      default: 1,
    });

    switch (methodologyOrRelease.type[0]) {
      case 'Methodology':
        // create a methodology content block
        const publicationExists = await prompt({
          name: 'exists',
          type: 'confirm',
          message: 'Do you have an existing publication?',
          prefix: '>',
        });

        if (publicationExists.exists) {
          const methodology = await prompt({
            name: 'id',
            type: 'input',
            message: 'methodology ID from existing publication',
            prefix: '>',
            validate: async input => {
              if (!Guid.isGuid(input)) {
                return 'Not a valid GUID';
              }
              return true;
            },
          });

          for (let i = 0; i < times.number; i += 1) {
            await addMethodlogyTextContentBlock(methodology.id);
          }
        }
        const { publicationId } = await createPublicationAndRelease();
        const methodologyId = await createMethodology(publicationId);
        for (let i = 0; i < times.number; i += 1) {
          await addMethodlogyTextContentBlock(methodologyId);
        }
        break;

      case 'Release':
        // eslint-disable-next-line no-case-declarations
        const releaseExists = await prompt({
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
          const existingRelease = await prompt({
            name: 'id',
            type: 'input',
            message: 'release ID from existing release',
            prefix: '>',
            validate: async input => {
              if (!Guid.isGuid(input)) {
                return 'Not a valid GUID';
              }
              return true;
            },
          });
          for (let i = 0; i < times.number; i += 1) {
            // eslint-disable-next-line no-await-in-loop
            await addReleaseTextContentBlock(existingRelease.id);
          }
        }
        break;

      default:
        // eslint-disable-next-line no-console
        logger.error(
          chalk.red('Invalid action:', methodologyOrRelease.type[0]),
        );
    }
  },
};

export default promptService;
