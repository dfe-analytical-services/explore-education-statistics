import createPublicationListRequest from '@frontend/modules/find-statistics/utils/createPublicationListRequest';
import { ParsedUrlQuery } from 'querystring';
import { UseQueryOptions } from '@tanstack/react-query';
import { PublicationListSummary } from '@common/services/publicationService';
import azurePublicationService, {
  PaginatedListWithAzureFacets,
} from '@frontend/services/azurePublicationService';

const azurePublicationQueries = {
  list(
    query: ParsedUrlQuery,
  ): UseQueryOptions<PaginatedListWithAzureFacets<PublicationListSummary>> {
    return {
      queryKey: ['listPublications', query],
      queryFn: async () =>
        azurePublicationService.listPublications(
          createPublicationListRequest(query),
        ),
    };
  },
} as const;

export default azurePublicationQueries;
