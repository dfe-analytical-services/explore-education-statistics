import publicationService, {
  PublicationListSummary,
} from '@common/services/publicationService';
import createPublicationListRequest from '@frontend/modules/find-statistics/utils/createPublicationListRequest';
import { ParsedUrlQuery } from 'querystring';
import { UseQueryOptions } from '@tanstack/react-query';
import { PaginatedList } from '@common/services/types/pagination';

const publicationQueries = {
  list(
    query: ParsedUrlQuery,
  ): UseQueryOptions<PaginatedList<PublicationListSummary>> {
    return {
      queryKey: ['listPublications', query],
      queryFn: () =>
        publicationService.listPublications(
          createPublicationListRequest(query),
        ),
    };
  },
} as const;

export default publicationQueries;
