/* eslint-disable no-await-in-loop */
import logger from '../../utils/logger';
import releaseVersionService from '../../services/releaseVersionService';

const createReleases = async (publicationId: string, amount: number) => {
  for (let i = 0; i < amount; i += 1) {
    await releaseVersionService.createRelease(publicationId);
  }

  logger.info(
    `Created ${amount} ${
      amount > 1 ? 'releases' : 'release'
    } for publication ${publicationId}`,
  );
};

export default createReleases;
