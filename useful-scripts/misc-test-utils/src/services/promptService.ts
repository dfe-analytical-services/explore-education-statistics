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
import addMethodologyTextContentBlock from '../modules/methodology/addMethodologyTextContentBlock';
import addReleaseTextContentBlock from '../modules/release/addContentBlock';
import logger from '../utils/logger';
import releaseService from './releaseService';
import subjectService from './subjectService';
import commonService from './commonService';
import publishAllReleases from '../modules/publication/publishAllReleases';
import createReleases from '../modules/release/createReleases';

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

    const numberOfReleasesToCreate = await prompt({
      name: 'number',
      type: 'number',
      message: 'How many releases do you want to create?',
      prefix: '>',
      validate: async (input: number) => {
        if (input < 1) {
          return 'Must be greater than 0';
        }
        return true;
      },
    });

    await createReleases(publication.id, numberOfReleasesToCreate.number);
  },

  publishAllReleases: async () => {
    const publication = await prompt({
      name: 'id',
      type: 'input',
      message: 'Enter publication id:',
      prefix: '>',
      validate: async (input: string) => {
        if (!Guid.isGuid(input)) {
          return 'Not a valid GUID';
        }
        return true;
      },
    });

    await publishAllReleases(publication.id);
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

  uploadManySubjectsAndPublish: async () => {
    const publication = await prompt({
      name: 'id',
      type: 'input',
      message: 'publication ID from existing publication',
      prefix: '>',
      validate: async (input: string) => {
        if (!Guid.isGuid(input)) {
          return 'Not a valid GUID';
        }
        return true;
      },
    });

    const numberOfReleases = await prompt({
      name: 'number',
      type: 'number',
      message: 'How many releases do you want to create?',
      prefix: '>',
      validate: async (input: number) => {
        if (input < 1) {
          return 'Must be greater than 0';
        }
        return true;
      },
    });
    await createReleases(publication.id, numberOfReleases.number);

    const numOfSubjects = await prompt({
      name: 'number',
      type: 'number',
      message: 'How many subjects do you want to upload?',
      prefix: '>',
      default: 1,
      validate: async (input: number) => {
        if (input < 1) {
          return 'Must be greater than 0';
        }
        return true;
      },
    });

    const releases = await releaseService.getAllReleases(publication.id);

    await commonService.prepareDirectories();

    // eslint-disable-next-line no-restricted-syntax
    for (const release of releases) {
      for (let i = 0; i < numOfSubjects.number; i += 1) {
        // eslint-disable-next-line no-await-in-loop
        await uploadSingleSubject(release.id, false);

        const subjectIdPair = await subjectService.getSubjectIdArr(release.id);

        await releaseService.addDataGuidance(subjectIdPair, release.id);
      }
    }

    const publishingMethod = await prompt({
      name: 'method',
      type: 'list',
      message: 'How would you like to publish the release?',
      prefix: '>',
      default: 'Immediate',
      choices: ['Immediate', 'Scheduled'],
    });

    await publishAllReleases(publication.id, publishingMethod.method);
  },

  uploadSubject: async () => {
    const release = await prompt({
      name: 'id',
      type: 'input',
      message: 'Release ID from existing publication',
      prefix: '>',
      validate: async (input: string) => {
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
      validate: async (input: number) => {
        if (input < 1) {
          return 'Must be greater than 0';
        }
        return true;
      },
    });

    const fast = await prompt({
      name: 'shouldBeFast',
      type: 'confirm',
      default: false,
      message:
        "exit as soon as the subject is in the 'QUEUED' status? (don't wait for it to finish uploading)",
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
      validate: async (input: string) => {
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
      message: 'What content block would you like to create?',
      prefix: '>',
      choices: ['Methodology', 'Release'],
    });

    const times = await prompt({
      name: 'number',
      type: 'number',
      message: 'How many content blocks do you want to create?',
      prefix: '>',
      default: 1,
      validate: async (input: number) => {
        if (input < 1) {
          return 'Must be greater than 0';
        }
        return true;
      },
    });

    switch (methodologyOrRelease.type[0]) {
      case 'Methodology':
        // create a methodology content block
        const publication = await prompt({
          name: 'exists',
          type: 'confirm',
          message: 'Do you have an existing publication?',
          prefix: '>',
        });

        if (publication.exists) {
          const methodology = await prompt({
            name: 'id',
            type: 'input',
            message: 'methodology ID from existing publication',
            prefix: '>',
            validate: async (input: string) => {
              if (!Guid.isGuid(input)) {
                return 'Not a valid GUID';
              }
              return true;
            },
          });

          for (let i = 0; i < times.number; i += 1) {
            await addMethodologyTextContentBlock(methodology.id);
          }
        }
        const { publicationId } = await createPublicationAndRelease();
        const methodologyId = await createMethodology(publicationId);
        for (let i = 0; i < times.number; i += 1) {
          await addMethodologyTextContentBlock(methodologyId);
        }
        break;

      case 'Release':
        // eslint-disable-next-line no-case-declarations
        const release = await prompt({
          name: 'exists',
          type: 'confirm',
          message: 'Do you have an existing release?',
          prefix: '>',
        });

        if (!release.exists) {
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
        logger.error(
          chalk.red('Invalid action:', methodologyOrRelease.type[0]),
        );
    }
  },
};

export default promptService;
