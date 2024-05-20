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
      queryKey: ['dataSet', dataSetId],
      queryFn: () => apiDataSetService.getDataSet(dataSetId),
    };
  },
  getDataSetVersion(
    dataSetId: string,
    dataSetVersion: string,
  ): UseQueryOptions<ApiDataSetVersion> {
    return {
      queryKey: ['dataSetVersion', dataSetId],
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
} as const;

export default apiDataSetQueries;
