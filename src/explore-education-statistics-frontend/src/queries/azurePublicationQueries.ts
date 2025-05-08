import createPublicationListRequest from '@frontend/modules/find-statistics/utils/createPublicationListRequest';
import { ParsedUrlQuery } from 'querystring';
import { UseQueryOptions } from '@tanstack/react-query';
import { PaginatedList } from '@common/services/types/pagination';
import { PublicationListSummary } from '@common/services/publicationService';
import azurePublicationService from '@frontend/services/azurePublicationService';

const azurePublicationQueries = {
  listAzure(
    query: ParsedUrlQuery,
  ): UseQueryOptions<PaginatedList<PublicationListSummary>> {
    return {
      queryKey: ['listPublicationsAzure', query],
      queryFn: async () =>
        azurePublicationService.listPublications(
          createPublicationListRequest(query),
        ),
    };
  },
} as const;

export default azurePublicationQueries;
