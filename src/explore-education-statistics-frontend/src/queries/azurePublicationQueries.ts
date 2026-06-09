import { PublicationListSummary } from '@common/services/publicationService';
import { PaginatedList } from '@common/services/types/pagination';
import createPublicationListRequest from '@frontend/modules/find-statistics/utils/createPublicationListRequest';
import createStatisticalReleasesListRequest from '@frontend/modules/search-data/utils/createStatisticalReleasesListRequest';
import azurePublicationService, {
  PaginatedListWithAzureFacets,
} from '@frontend/services/azurePublicationService';
import { UseQueryOptions } from '@tanstack/react-query';
import { ParsedUrlQuery } from 'querystring';

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
  // This is used for the search data page prototype, separate to the find stats page
  // as it accepts multiple filter items and doesn't need facets
  listStatisticalReleases(
    query: ParsedUrlQuery,
  ): UseQueryOptions<PaginatedList<PublicationListSummary>> {
    return {
      queryKey: ['listPublications', query],
      queryFn: async () =>
        azurePublicationService.listStatisticalReleases(
          createStatisticalReleasesListRequest(query),
        ),
    };
  },
} as const;

export default azurePublicationQueries;
