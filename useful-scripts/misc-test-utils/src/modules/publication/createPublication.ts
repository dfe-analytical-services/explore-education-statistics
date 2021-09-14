import publicationService from '../../services/publicationService';

const createPublication = async () => {
  const publicationId = await publicationService.createPublication();
  return publicationId;
};
export default createPublication;
