import publicationService, {
  PublicationMethodologiesList,
  PublicationReleaseSeriesItem,
  PublicationSummaryRedesign,
  PublicationTreeOptions,
  ReleaseSummary,
  ReleaseVersion,
  ReleaseVersionSummary,
  Theme,
} from '@common/services/publicationService';
import {
  PaginatedList,
  PaginationRequestParams,
} from '@common/services/types/pagination';
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
  getPublicationSummaryRedesign(
    publicationSlug: string,
  ): UseQueryOptions<PublicationSummaryRedesign> {
    return {
      queryKey: ['publicationSummaryRedesign', publicationSlug],
      queryFn: () =>
        publicationService.getPublicationSummaryRedesign(publicationSlug),
    };
  },
  getReleaseVersionSummary(
    publicationSlug: string,
    releaseSlug: string,
  ): UseQueryOptions<ReleaseVersionSummary> {
    return {
      queryKey: ['releaseVersionSummary', publicationSlug],
      queryFn: () =>
        publicationService.getReleaseVersionSummary(
          publicationSlug,
          releaseSlug,
        ),
    };
  },
  getPublicationMethodologies(
    publicationSlug: string,
  ): UseQueryOptions<PublicationMethodologiesList[]> {
    return {
      queryKey: ['publicationMethodologies', publicationSlug],
      queryFn: () =>
        publicationService.getPublicationMethodologies(publicationSlug),
    };
  },
  getPublicationReleaseList(
    publicationSlug: string,
    params?: PaginationRequestParams,
  ): UseQueryOptions<PaginatedList<PublicationReleaseSeriesItem>> {
    return {
      queryKey: ['publicationReleaseList', publicationSlug, params ?? null],
      queryFn: () =>
        publicationService.getPublicationReleaseList(publicationSlug, params),
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
