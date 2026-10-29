import tableBuilderQueries from '@common/queries/tableBuilderQueries';
import logger from '@common/services/logger';
import getSsrDataBlocks, {
  SsrDataBlocksHomeContent,
} from '@frontend/modules/find-statistics/utils/getSsrDataBlocks';
import { dehydrate, DehydratedState, QueryClient } from '@tanstack/react-query';

// Bounds how long these fetches can delay the page. Anything still pending is
// left for the client to fetch, as it would be without server-side fetching.
const defaultTimeoutMs = 2000;

interface Options {
  homeContent: SsrDataBlocksHomeContent;
  releaseVersionId: string;
  timeoutMs?: number;
}

/**
 * Fetch the data for a release home page's data blocks, so that they render
 * server-side. Returns the dehydrated query cache to pass to the page as
 * `dehydratedState`, so the client hydrates the same data.
 *
 * Never throws: a failed or slow data block is left for the client to fetch,
 * rather than failing the page.
 */
export default async function prefetchSsrDataBlocks({
  homeContent,
  releaseVersionId,
  timeoutMs = defaultTimeoutMs,
}: Options): Promise<DehydratedState> {
  // A dedicated client, so it only ever holds these data blocks. The page's
  // other data is already passed as props.
  const queryClient = new QueryClient();

  const { keyStatIds, tableIds } = getSsrDataBlocks(homeContent);

  const fetches = [
    ...keyStatIds.map(dataBlockId =>
      queryClient.fetchQuery(
        tableBuilderQueries.getKeyStat(releaseVersionId, dataBlockId),
      ),
    ),
    ...tableIds.map(dataBlockId =>
      queryClient.fetchQuery(
        tableBuilderQueries.getDataBlockTable(releaseVersionId, dataBlockId),
      ),
    ),
  ];

  let timeout: ReturnType<typeof setTimeout> | undefined;

  await Promise.race([
    Promise.all(
      fetches.map(request =>
        // The page's `getServerSideProps` turns any error into a 404.
        request.catch(error => logger.error(error)),
      ),
    ),
    new Promise(resolve => {
      timeout = setTimeout(resolve, timeoutMs);
    }),
  ]);

  clearTimeout(timeout);

  return dehydrate(queryClient);
}
