import chalk from 'chalk';
import releaseService from '../../services/releaseService';
import publicationService from '../../services/publicationService';

const createPublicationAndRelease = async (): Promise<{
  publicationId: string;
  releaseId: string;
}> => {
  const publicationId: string = await publicationService.createPublication();
  const releaseId = await releaseService.createRelease(publicationId);

  if (!releaseId) {
    throw new Error(
      chalk.red(
        'No releaseId returned from "releaseService.createRelease" function!',
      ),
    );
  }
  return {
    publicationId,
    releaseId,
  };
};
export default createPublicationAndRelease;
