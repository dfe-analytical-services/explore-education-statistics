import { createQueryKeys } from '@lukemorales/query-key-factory';
import publicationService, {
  ReleaseVersionsType,
} from '@admin/services/publicationService';
import { ReleaseVersionSummaryWithPermissions } from '@admin/services/releaseVersionService';

const publicationQueries = createQueryKeys('publication', {
  get(publicationId: string) {
    return {
      queryKey: [publicationId],
      queryFn: () => publicationService.getPublication(publicationId),
    };
  },
  getReleaseSeries(publicationId: string) {
    return {
      queryKey: [publicationId],
      queryFn: () => publicationService.getReleaseSeries(publicationId),
    };
  },
  getPublicationSummaries: {
    queryKey: null,
    queryFn: () => publicationService.getPublicationSummaries(),
  },
  listPublishedReleaseVersions(
    publicationId: string,
    pageSize: number = 5,
    includePermissions: boolean = true,
  ) {
    return {
      queryKey: [publicationId],
      queryFn: ({ pageParam = 1 }) =>
        publicationService.listReleaseVersions<ReleaseVersionSummaryWithPermissions>(
          publicationId,
          {
            versionsType: ReleaseVersionsType.LatestPublished,
            page: pageParam,
            pageSize,
            includePermissions,
          },
        ),
    };
  },
  listUnpublishedReleaseVersions(
    publicationId: string,
    pageSize: number = 100,
    includePermissions: boolean = true,
  ) {
    return {
      queryKey: [publicationId],
      queryFn: () =>
        publicationService.listReleaseVersions<ReleaseVersionSummaryWithPermissions>(
          publicationId,
          {
            versionsType: ReleaseVersionsType.NotPublished,
            pageSize,
            includePermissions,
          },
        ),
    };
  },
});

export default publicationQueries;
