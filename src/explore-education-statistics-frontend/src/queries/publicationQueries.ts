import publicationService, {
  PublicationTreeOptions,
  ReleaseSummary,
  ReleaseVersion,
  Theme,
} from '@common/services/publicationService';
import { UseQueryOptions } from '@tanstack/react-query';

const publicationQueries = {
  getLatestPublicationRelease(
    publicationSlug: string,
  ): UseQueryOptions<ReleaseVersion> {
    return {
      queryKey: ['latestPublicationRelease', publicationSlug],
      queryFn: () =>
        publicationService.getLatestPublicationRelease(publicationSlug),
    };
  },
  getPublicationTree(query: PublicationTreeOptions): UseQueryOptions<Theme[]> {
    return {
      queryKey: ['publicationTree', query],
      queryFn: () => publicationService.getPublicationTree(query),
    };
  },
  listReleases(publicationSlug: string): UseQueryOptions<ReleaseSummary[]> {
    return {
      queryKey: ['listReleases', publicationSlug],
      queryFn: () => publicationService.listReleases(publicationSlug),
    };
  },
} as const;

export default publicationQueries;
