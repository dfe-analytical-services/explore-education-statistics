import releaseUpdatesService, {
  ReleaseUpdate,
} from '@common/services/releaseUpdatesService';
import {
  PaginatedList,
  PaginationRequestParams,
} from '@common/services/types/pagination';
import { UseQueryOptions } from '@tanstack/react-query';

const releaseUpdatesQueries = {
  getReleaseUpdates(
    publicationSlug: string,
    releaseSlug: string,
    params?: PaginationRequestParams,
  ): UseQueryOptions<PaginatedList<ReleaseUpdate>> {
    return {
      queryKey: [
        'releaseUpdates',
        publicationSlug,
        releaseSlug,
        params ?? null,
      ],
      queryFn: () =>
        releaseUpdatesService.getReleaseUpdates(
          publicationSlug,
          releaseSlug,
          params,
        ),
    };
  },
} as const;

export default releaseUpdatesQueries;
