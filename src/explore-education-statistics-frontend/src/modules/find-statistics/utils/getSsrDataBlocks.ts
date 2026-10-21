import {
  KeyStatisticDataBlock,
  KeyStatisticTypes,
  ReleaseVersionHomeContent,
} from '@common/services/publicationService';

/**
 * Get the ids of the data blocks on a release's home page whose data should
 * be fetched server-side, so that they are in the initial HTML.
 */
export default function getSsrDataBlocks(
  homeContent: Pick<ReleaseVersionHomeContent, 'keyStatistics'>,
) {
  const keyStatIds = homeContent.keyStatistics
    .filter(
      (keyStat): keyStat is KeyStatisticDataBlock =>
        keyStat.type === KeyStatisticTypes.DataBlock,
    )
    .map(keyStat => keyStat.dataBlockId);

  return { keyStatIds };
}
