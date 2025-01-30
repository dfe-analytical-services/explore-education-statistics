/* eslint-disable no-console */
import chalk from 'chalk';
import logger from '../../utils/logger';
import releaseVersionService from '../../services/releaseVersionService';

const addReleaseTextContentBlock = async (releaseId: string) => {
  const sectionId = await releaseVersionService.addContentSection(releaseId);

  if (!sectionId) {
    throw new Error(
      chalk.red(
        'No sectionId was returning from the "releaseVersionService.addContentSection" function!',
      ),
    );
  }
  const blockId = await releaseVersionService.addTextBlock(
    releaseId,
    sectionId,
  );

  if (!blockId) {
    throw new Error(
      chalk.red(
        'No blockId returned from "releaseVersionService.addTextBlock" function!',
      ),
    );
  }
  await releaseVersionService.addTextContent(releaseId, sectionId, blockId);
  logger.info(chalk.green('Successfully added release content block'));
};
export default addReleaseTextContentBlock;
