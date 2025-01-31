/* eslint-disable no-console */
import chalk from 'chalk';
import logger from '../../utils/logger';
import releaseService from '../../services/releaseService';

const addReleaseTextContentBlock = async (releaseId: string) => {
  const sectionId = await releaseService.addContentSection(releaseId);

  if (!sectionId) {
    throw new Error(
      chalk.red(
        'No sectionId was returning from the "releaseService.addContentSection" function!',
      ),
    );
  }
  const blockId = await releaseService.addTextBlock(releaseId, sectionId);

  if (!blockId) {
    throw new Error(
      chalk.red(
        'No blockId returned from "releaseService.addTextBlock" function!',
      ),
    );
  }
  await releaseService.addTextContent(releaseId, sectionId, blockId);
  logger.info(chalk.green('Successfully added release content block'));
};
export default addReleaseTextContentBlock;
