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
  listPublishedReleaseVersionsWithPermissions(
    publicationId: string,
    pageSize: number = 5,
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
            includePermissions: true,
          },
        ),
    };
  },
  listUnpublishedReleaseVersionsWithPermissions(
    publicationId: string,
    pageSize: number = 100,
  ) {
    return {
      queryKey: [publicationId],
      queryFn: () =>
        publicationService.listReleaseVersions<ReleaseVersionSummaryWithPermissions>(
          publicationId,
          {
            versionsType: ReleaseVersionsType.OnlyDraft,
            pageSize,
            includePermissions: true,
          },
        ),
    };
  },
});

export default publicationQueries;
