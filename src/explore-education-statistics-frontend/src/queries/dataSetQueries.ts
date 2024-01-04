import { PaginatedList } from '@common/services/types/pagination';
import dataSetService, {
  DataSetSummary,
} from '@frontend/services/dataSetService';
import createDataSetListRequest from '@frontend/modules/data-catalogue/utils/createDataSetListRequest';
import { ParsedUrlQuery } from 'querystring';
import { UseQueryOptions } from '@tanstack/react-query';

const dataSetQueries = {
  list(query: ParsedUrlQuery): UseQueryOptions<PaginatedList<DataSetSummary>> {
    return {
      queryKey: ['listDataSets', query],
      queryFn: () =>
        dataSetService.listDataSets(createDataSetListRequest(query)),
    };
  },
} as const;

export default dataSetQueries;
