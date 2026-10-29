import {
  DataBlockViewModel,
  KeyStatisticDataBlock,
  KeyStatisticTypes,
  ReleaseVersionHomeContent,
} from '@common/services/publicationService';

// The most data block tables to fetch server-side for a page.
// Capping them because each table is dehydrated in full (adding page weight)
// and costs a data API request whenever the page isn't cached.
// Key stats are uncapped.
export const MAX_TABLES_TO_SSR = 3;

export type SsrDataBlocksHomeContent = Pick<
  ReleaseVersionHomeContent,
  'content' | 'keyStatistics' | 'keyStatisticsSecondarySection'
>;

/**
 * Get the ids of the data blocks on a release's home page whose data should
 * be fetched server-side, so that they are in the initial HTML.
 */
export default function getSsrDataBlocks(
  homeContent: SsrDataBlocksHomeContent,
) {
  const keyStatIds = homeContent.keyStatistics
    .filter(
      (keyStat): keyStat is KeyStatisticDataBlock =>
        keyStat.type === KeyStatisticTypes.DataBlock,
    )
    .map(keyStat => keyStat.dataBlockId);

  const tableIds = [
    // First, as it wraps the whole headlines section, key stats included.
    ...homeContent.keyStatisticsSecondarySection.content,
    ...homeContent.content.flatMap(section => section.content),
  ]
    // `DataBlockTabs` only renders a table when there is one.
    .filter(
      (block): block is DataBlockViewModel =>
        block.type === 'DataBlockVersionLink' && !!block.dataBlockVersion.table,
    )
    .map(block => block.dataBlockVersion.dataBlockId)
    .slice(0, MAX_TABLES_TO_SSR);

  return { keyStatIds, tableIds };
}
