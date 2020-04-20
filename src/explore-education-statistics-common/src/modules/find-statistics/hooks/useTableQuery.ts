import { useErrorControl } from '@common/contexts/ErrorControlContext';
import useAsyncRetry, { AsyncRetryState } from '@common/hooks/useAsyncRetry';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import tableBuilderService, {
  TableDataQuery,
} from '@common/services/tableBuilderService';

export default function useTableQuery(
  query: TableDataQuery | undefined,
): AsyncRetryState<FullTable | undefined> {
  const { withoutErrorHandling } = useErrorControl();

  return useAsyncRetry(async () => {
    if (!query) {
      return undefined;
    }

    return withoutErrorHandling(async () => {
      const response = await tableBuilderService.getTableData(query);
      return mapFullTable(response);
    });
  }, [JSON.stringify(query)]);
}
