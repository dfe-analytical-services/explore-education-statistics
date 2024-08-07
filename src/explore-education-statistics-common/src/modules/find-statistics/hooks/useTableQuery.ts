import useAsyncRetry, { AsyncRetryState } from '@common/hooks/useAsyncRetry';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import tableBuilderService from '@common/services/tableBuilderService';

export default function useTableQuery(
  releaseId: string,
  dataBlockParentId: string,
  boundaryLevelId?: number | undefined,
): AsyncRetryState<FullTable> {
  return useAsyncRetry(async () => {
    const response = await tableBuilderService.getDataBlockTableData(
      releaseId,
      dataBlockParentId,
      boundaryLevelId,
    );

    return mapFullTable(response);
  }, [releaseId, dataBlockParentId]);
}
