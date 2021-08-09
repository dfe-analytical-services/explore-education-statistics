import releaseService from '../../services/releaseService';
import publicationService from '../../services/publicationService';

const createPublicationAndRelease = async () => {
  const publicationId: string = await publicationService.createPublication();
  await releaseService.createRelease(publicationId);
};
export default createPublicationAndRelease;
