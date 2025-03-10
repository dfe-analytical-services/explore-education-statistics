import chalk from 'chalk';
import releaseVersionService from '../../services/releaseVersionService';
import publicationService from '../../services/publicationService';

const createPublicationAndRelease = async (): Promise<{
  publicationId: string;
  releaseId: string;
}> => {
  const publicationId: string = await publicationService.createPublication();
  const releaseId = await releaseVersionService.createRelease(publicationId);

  if (!releaseId) {
    throw new Error(
      chalk.red(
        'No releaseId returned from "releaseVersionService.createRelease" function!',
      ),
    );
  }
  return {
    publicationId,
    releaseId,
  };
};
export default createPublicationAndRelease;
