import useAsyncRetry, { AsyncRetryState } from '@common/hooks/useAsyncRetry';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import storageService from '@common/services/storageService';
import tableBuilderService, {
  TableDataQuery,
  TableDataResponse,
} from '@common/services/tableBuilderService';
import { addSeconds } from 'date-fns';
import { useRef } from 'react';

export interface TableQueryOptions {
  /**
   * How long to cache the table
   * query response for, in seconds.
   */
  expiresIn?: number;
}

export default function useTableQuery(
  query: TableDataQuery | undefined,
  releaseId: string | undefined,
  options: TableQueryOptions = {},
  dataLastPublished: string,
): AsyncRetryState<FullTable | undefined> {
  const optionsRef = useRef<TableQueryOptions | undefined>(options);
  optionsRef.current = options;

  return useAsyncRetry(async () => {
    if (!query) {
      return undefined;
    }

    const queryKey = JSON.stringify({
      ...query,
      dataLastPublished,
    });

    let response = await storageService
      .get<TableDataResponse>(queryKey)
      .catch(() => null);

    if (!response) {
      response = await tableBuilderService.getTableData(query, releaseId);

      await storageService.set(queryKey, response, {
        expiry: addSeconds(new Date(), options?.expiresIn ?? 0),
      });
    }

    return mapFullTable(response);
  }, [JSON.stringify(query)]);
}
