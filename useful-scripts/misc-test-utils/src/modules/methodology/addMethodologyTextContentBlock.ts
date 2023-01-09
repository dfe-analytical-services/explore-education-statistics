/* eslint-disable no-console */
import chalk from 'chalk';
import spinner from '../../utils/spinner';
import methodologyService from '../../services/methodologyService';

const addMethodologyTextContentBlock = async (methodologyId: string) => {
  spinner.start();
  const sectionId = await methodologyService.addContentSection(methodologyId);
  if (!sectionId) {
    throw new Error(
      chalk.red(
        'No sectionId returned from "methodologyService.addContentSection" function!',
      ),
    );
  }
  const blockId = await methodologyService.addTextBlock(
    methodologyId,
    sectionId,
  );

  if (!blockId) {
    throw new Error(
      chalk.red(
        'No blockId returned from "methodologyService.addTextBlock" function!',
      ),
    );
  }

  await methodologyService.addContent(methodologyId, sectionId, blockId);
  spinner.succeed('successfully added methodology content block');
};
export default addMethodologyTextContentBlock;
