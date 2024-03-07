import { PaginatedList } from '@common/services/types/pagination';
import dataSetService, {
  DataSet,
  DataSetSummary,
} from '@frontend/services/dataSetService';
import createDataSetListRequest from '@frontend/modules/data-catalogue/utils/createDataSetListRequest';
import { ParsedUrlQuery } from 'querystring';
import { UseQueryOptions } from '@tanstack/react-query';

const dataSetQueries = {
  get(dataSetId: string, releaseId: string): UseQueryOptions<DataSet> {
    return {
      queryKey: [dataSetId],
      queryFn: () => dataSetService.getDataSet(dataSetId, releaseId),
    };
  },
  list(query: ParsedUrlQuery): UseQueryOptions<PaginatedList<DataSetSummary>> {
    return {
      queryKey: ['listDataSets', query],
      queryFn: () =>
        dataSetService.listDataSets(createDataSetListRequest(query)),
    };
  },
} as const;

export default dataSetQueries;
