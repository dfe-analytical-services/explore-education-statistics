/* eslint-disable no-await-in-loop */
import logger from '../../utils/logger';
import releaseService from '../../services/releaseService';

export type PublishMethod = 'Immediate' | 'Scheduled';

const publishAllReleases = async (
  publicationId: string,
  publishMethod: PublishMethod = 'Immediate',
) => {
  const releases = await releaseService.getAllReleases(publicationId);

  switch (publishMethod) {
    case 'Immediate':
      logger.info('Publishing all releases immediately');
      // eslint-disable-next-line no-case-declarations
      const publishedReleases = new Set<string>();

      for (let i = 0; i < releases.length; i += 1) {
        const release = await releaseService.getRelease(releases[i].id);
        await releaseService.publishRelease(
          {
            ...release,
            approvalStatus: 'Approved',
            publishMethod: 'Immediate',
          },
          release.id,
        );

        publishedReleases.add(release.id);
      }

      logger.info('All releases:', publishedReleases);
      break;

    case 'Scheduled':
      logger.info('Scheduling all releases to be published for tomorrow');
      // eslint-disable-next-line no-case-declarations
      const scheduledReleases = new Set<string>();
      for (let i = 0; i < releases.length; i += 1) {
        const release = await releaseService.getRelease(releases[i].id);

        const tomorrow = new Date(new Date().getTime() + 24 * 60 * 60 * 1000)
          .toISOString()
          .split('T')[0];

        await releaseService.publishRelease(
          {
            ...release,
            approvalStatus: 'Approved',
            publishScheduled: tomorrow,
            publishMethod: 'Scheduled',
          },
          release.id,
        );

        scheduledReleases.add(release.id);
      }

      logger.info('All scheduled releases:', scheduledReleases);
      break;

    default:
      logger.error('Invalid publishing method');
  }
};

export default publishAllReleases;
