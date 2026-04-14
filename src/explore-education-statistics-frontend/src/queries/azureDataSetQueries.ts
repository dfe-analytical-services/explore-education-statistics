import { ParsedUrlQuery } from 'querystring';
import { UseQueryOptions } from '@tanstack/react-query';
import { PaginatedListWithAzureFacets } from '@frontend/services/azurePublicationService';
import azureDataSetService from '@frontend/services/azureDataSetService';
import createDataSetListRequest from '@frontend/modules/search-data/utils/createDataSetListRequest';
import { DataSetFileSummary } from '@frontend/services/dataSetFileService';

const azureDataSetQueries = {
  list(
    query: ParsedUrlQuery,
  ): UseQueryOptions<PaginatedListWithAzureFacets<DataSetFileSummary>> {
    return {
      queryKey: ['listDataSets', query],
      queryFn: async () =>
        azureDataSetService.listDataSets(createDataSetListRequest(query)),
    };
  },
} as const;

export default azureDataSetQueries;
