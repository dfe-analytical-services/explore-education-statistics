import useAsyncRetry, { AsyncRetryState } from '@common/hooks/useAsyncRetry';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import storageService from '@common/services/storageService';
import tableBuilderService, {
  ReleaseTableDataQuery,
  TableDataResponse,
} from '@common/services/tableBuilderService';
import { addSeconds } from 'date-fns';
import { useRef } from 'react';

export interface TableQueryOptions {
  /**
   * When was the data last published.
   * This is used for cache invalidation.
   */
  dataLastPublished?: string;
  /**
   * How long to cache the table
   * query response for, in seconds.
   */
  expiresIn?: number;
}

export default function useTableQuery(
  query: ReleaseTableDataQuery,
  options: TableQueryOptions = {},
): AsyncRetryState<FullTable> {
  const optionsRef = useRef<TableQueryOptions>(options);
  optionsRef.current = options;

  return useAsyncRetry(async () => {
    const { dataLastPublished, expiresIn } = optionsRef.current;

    const queryKey = JSON.stringify({
      ...query,
      dataLastPublished,
    });

    let response = await storageService
      .get<TableDataResponse>(queryKey)
      .catch(() => null);

    if (!response) {
      response = await tableBuilderService.getTableData(query);

      await storageService.set(queryKey, response, {
        expiry: addSeconds(new Date(), expiresIn ?? 0),
      });
    }

    return mapFullTable(response);
  }, [JSON.stringify(query)]);
}
