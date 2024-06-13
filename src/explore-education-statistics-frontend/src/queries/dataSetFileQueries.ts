import { PaginatedList } from '@common/services/types/pagination';
import dataSetFileService, {
  DataSetFile,
  DataSetFileListRequest,
  DataSetFileSummary,
} from '@frontend/services/dataSetFileService';
import { UseQueryOptions } from '@tanstack/react-query';

const dataSetFileQueries = {
  get(dataSetFileId: string): UseQueryOptions<DataSetFile> {
    return {
      queryKey: [dataSetFileId],
      queryFn: () => dataSetFileService.getDataSetFile(dataSetFileId),
    };
  },
  list(
    query: DataSetFileListRequest,
  ): UseQueryOptions<PaginatedList<DataSetFileSummary>> {
    return {
      queryKey: ['listDataSetFiles', query],
      queryFn: () => dataSetFileService.listDataSetFiles(query),
    };
  },
} as const;

export default dataSetFileQueries;
