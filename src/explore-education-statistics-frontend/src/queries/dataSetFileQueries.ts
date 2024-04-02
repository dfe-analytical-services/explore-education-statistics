import { PaginatedList } from '@common/services/types/pagination';
import dataSetFileService, {
  DataSetFile,
  DataSetFileSummary,
} from '@frontend/services/dataSetFileService';
import createDataSetFileListRequest from '@frontend/modules/data-catalogue/utils/createDataSetFileListRequest';
import { ParsedUrlQuery } from 'querystring';
import { UseQueryOptions } from '@tanstack/react-query';

const dataSetFileQueries = {
  get(dataSetFileId: string): UseQueryOptions<DataSetFile> {
    return {
      queryKey: [dataSetFileId],
      queryFn: () => dataSetFileService.getDataSetFile(dataSetFileId),
    };
  },
  list(
    query: ParsedUrlQuery,
  ): UseQueryOptions<PaginatedList<DataSetFileSummary>> {
    return {
      queryKey: ['listDataSetFiles', query],
      queryFn: () =>
        dataSetFileService.listDataSetFiles(
          createDataSetFileListRequest(query),
        ),
    };
  },
} as const;

export default dataSetFileQueries;
