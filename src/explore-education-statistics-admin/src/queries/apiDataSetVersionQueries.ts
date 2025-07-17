import apiDataSetVersionService, {
  ListVersionsParams,
} from '@admin/services/apiDataSetVersionService';
import { createQueryKeys } from '@lukemorales/query-key-factory';

const apiDataSetVersionQueries = createQueryKeys('apiDataSetVersionQueries', {
  list(query: ListVersionsParams) {
    return {
      queryKey: [query],
      queryFn: () => apiDataSetVersionService.listVersions(query),
    };
  },
  getVersion(dataSetVersionId: string) {
    return {
      queryKey: [dataSetVersionId],
      queryFn: () => apiDataSetVersionService.getVersion(dataSetVersionId),
    };
  },
  getFiltersMapping(dataSetVersionId: string) {
    return {
      queryKey: [dataSetVersionId],
      queryFn: () =>
        apiDataSetVersionService.getFiltersMapping(dataSetVersionId),
    };
  },
  getLocationsMapping(dataSetVersionId: string) {
    return {
      queryKey: [dataSetVersionId],
      queryFn: () =>
        apiDataSetVersionService.getLocationsMapping(dataSetVersionId),
    };
  },
  getChanges(dataSetVersionId: string) {
    return {
      queryKey: [dataSetVersionId],
      queryFn: () => apiDataSetVersionService.getChanges(dataSetVersionId),
    };
  },
});

export default apiDataSetVersionQueries;
