import useAsyncRetry, { AsyncRetryState } from '@common/hooks/useAsyncRetry';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import tableBuilderService from '@common/services/tableBuilderService';

export default function useTableQuery(
  releaseId: string,
  dataBlockParentId: string,
  boundaryLevel?: number
): AsyncRetryState<FullTable> {
  return  useAsyncRetry(async () => {
    const [tableData, locationGeoJson] = await Promise.all([
      tableBuilderService.getDataBlockTableData(
        releaseId,
        dataBlockParentId,
      ),
      boundaryLevel !== undefined ? tableBuilderService.getLocationGeoJson(releaseId, dataBlockParentId, boundaryLevel): undefined
    ])

    

    return mapFullTable({...tableData, subjectMeta: {...tableData.subjectMeta, locations: locationGeoJson ?? tableData.subjectMeta.locations}});
  }, [releaseId, dataBlockParentId]);
}
