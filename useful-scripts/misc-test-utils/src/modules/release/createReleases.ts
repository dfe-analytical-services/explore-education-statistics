/* eslint-disable no-await-in-loop */
import logger from '../../utils/logger';
import releaseService from '../../services/releaseService';

const createReleases = async (publicationId: string, amount: number) => {
  for (let i = 0; i < amount; i += 1) {
    await releaseService.createRelease(publicationId);
  }

  logger.info(
    `Created ${amount} ${
      amount > 1 ? 'releases' : 'release'
    } for publication ${publicationId}`,
  );
};

export default createReleases;
