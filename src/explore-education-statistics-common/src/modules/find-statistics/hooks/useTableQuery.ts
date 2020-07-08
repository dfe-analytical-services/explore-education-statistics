import useAsyncRetry, { AsyncRetryState } from '@common/hooks/useAsyncRetry';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import storageService from '@common/services/storageService';
import tableBuilderService, {
  TableDataQuery,
  TableDataResponse,
  TableDataResult,
  TableDataSubjectMeta,
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

export interface TableDataStorageItem {
  dataLastPublished: string;
  response: TableDataResponse;
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

    const queryKey = JSON.stringify(query);

    let item = await storageService
      .get<TableDataStorageItem>(queryKey)
      .catch(() => null);

    const dataExpired =
      item != null &&
      dataLastPublished !== '' &&
      new Date(dataLastPublished) > new Date(item.dataLastPublished);

    if (item == null || dataExpired) {
      const response = await tableBuilderService.getTableData(query, releaseId);

      item = {
        dataLastPublished,
        response,
      };

      await storageService.set(queryKey, item, {
        expiry: addSeconds(new Date(), options?.expiresIn ?? 0),
      });
    }

    return mapFullTable(item.response);
  }, [JSON.stringify(query)]);
}
