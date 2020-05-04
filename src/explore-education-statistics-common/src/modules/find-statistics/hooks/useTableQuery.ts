import { useErrorControl } from '@common/contexts/ErrorControlContext';
import useAsyncRetry, { AsyncRetryState } from '@common/hooks/useAsyncRetry';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import tableBuilderService, {
  TableDataQuery,
} from '@common/services/tableBuilderService';
import { useRef } from 'react';

interface TableQueryOptions {
  onSuccess?: (table: FullTable, query: TableDataQuery) => void;
  onError?: (error: Error) => void;
}

export default function useTableQuery(
  query: TableDataQuery | undefined,
  options?: TableQueryOptions,
): AsyncRetryState<FullTable | undefined> {
  const optionsRef = useRef<TableQueryOptions | undefined>(options);

  optionsRef.current = options;

  const { withoutErrorHandling } = useErrorControl();

  return useAsyncRetry(async () => {
    if (!query) {
      return undefined;
    }

    return withoutErrorHandling(async () => {
      try {
        const response = await tableBuilderService.getTableData(query);
        const fullTable = mapFullTable(response);

        if (optionsRef.current?.onSuccess) {
          await optionsRef.current.onSuccess(fullTable, query);
        }

        return fullTable;
      } catch (error) {
        if (optionsRef.current?.onError) {
          await optionsRef.current.onError(error);
        }

        throw error;
      }
    });
  }, [JSON.stringify(query)]);
}
