import createAzurePublicationListRequest, {
  createAzurePublicationSuggestRequest,
} from '@frontend/modules/find-statistics/utils/createAzurePublicationListRequest';
import { ParsedUrlQuery } from 'querystring';
import { UseQueryOptions } from '@tanstack/react-query';
import { PublicationListSummary } from '@common/services/publicationService';
import azurePublicationService, {
  AzurePublicationSuggestResult,
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
  // TODO use or remove
  suggestPublications(
    query: ParsedUrlQuery,
    searchTerm: string,
  ): UseQueryOptions<AzurePublicationSuggestResult[]> {
    return {
      queryKey: ['suggestPublicationsAzure', query, searchTerm],
      queryFn: async () =>
        azurePublicationService.suggestPublications(
          createAzurePublicationSuggestRequest(query, searchTerm),
        ),
    };
  },
} as const;

export default azurePublicationQueries;
