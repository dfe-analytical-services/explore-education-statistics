import { ApiDataSetVersionChanges } from '@common/services/types/apiDataSetChanges';
import { PaginatedList } from '@common/services/types/pagination';
import apiDataSetService, {
  ApiDataSet,
  ApiDataSetVersion,
  ApiDataSetVersionsListRequest,
} from '@frontend/services/apiDataSetService';
import { UseQueryOptions } from '@tanstack/react-query';

const apiDataSetQueries = {
  getDataSet(dataSetId: string): UseQueryOptions<ApiDataSet> {
    return {
      queryKey: ['getDataSet', dataSetId],
      queryFn: () => apiDataSetService.getDataSet(dataSetId),
    };
  },
  getDataSetVersion(
    dataSetId: string,
    dataSetVersion: string,
  ): UseQueryOptions<ApiDataSetVersion> {
    return {
      queryKey: ['getDataSetVersion', dataSetId, dataSetVersion],
      queryFn: () =>
        apiDataSetService.getDataSetVersion(dataSetId, dataSetVersion),
    };
  },
  listDataSetVersions(
    dataSetId: string,
    params?: ApiDataSetVersionsListRequest,
  ): UseQueryOptions<PaginatedList<ApiDataSetVersion>> {
    return {
      queryKey: ['listDataSetVersions', dataSetId, params ?? null],
      queryFn: () => apiDataSetService.listDataSetVersions(dataSetId, params),
    };
  },
  getDataSetVersionChanges(
    dataSetId: string,
    dataSetVersion: string,
    patchHistory: boolean = false,
  ): UseQueryOptions<ApiDataSetVersionChanges> {
    return {
      queryKey: ['getDataSetVersionChanges', dataSetId, dataSetVersion],
      queryFn: () =>
        apiDataSetService.getDataSetVersionChanges(
          dataSetId,
          dataSetVersion,
          patchHistory,
        ),
    };
  },
} as const;

export default apiDataSetQueries;
