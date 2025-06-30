import createAzurePublicationListRequest from '@frontend/modules/find-statistics/utils/createAzurePublicationListRequest';
import { ParsedUrlQuery } from 'querystring';
import { UseQueryOptions } from '@tanstack/react-query';
import { PublicationListSummary } from '@common/services/publicationService';
import azurePublicationService, {
  PaginatedListWithAzureFacets,
} from '@frontend/services/azurePublicationService';

const azurePublicationQueries = {
  listAzure(
    query: ParsedUrlQuery,
  ): UseQueryOptions<PaginatedListWithAzureFacets<PublicationListSummary>> {
    return {
      queryKey: ['listPublicationsAzure', query],
      queryFn: async () =>
        azurePublicationService.listPublications(
          createAzurePublicationListRequest(query),
        ),
    };
  },
} as const;

export default azurePublicationQueries;
