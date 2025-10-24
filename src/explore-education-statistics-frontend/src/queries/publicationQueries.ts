import publicationService, {
  PreReleaseAccessListSummary,
  PublicationMethodologiesList,
  PublicationReleaseSeriesItem,
  PublicationSummaryRedesign,
  PublicationTreeOptions,
  RelatedInformationItem,
  ReleaseSummary,
  ReleaseVersion,
  ReleaseVersionDataContent,
  ReleaseVersionHomeContent,
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
      queryKey: ['releaseVersionSummary', publicationSlug, releaseSlug],
      queryFn: () =>
        publicationService.getReleaseVersionSummary(
          publicationSlug,
          releaseSlug,
        ),
    };
  },
  getReleaseVersionHomeContent(
    publicationSlug: string,
    releaseSlug: string,
  ): UseQueryOptions<ReleaseVersionHomeContent> {
    return {
      queryKey: ['releaseVersionHomeContent', publicationSlug, releaseSlug],
      queryFn: () =>
        publicationService.getReleaseVersionHomeContent(
          publicationSlug,
          releaseSlug,
        ),
    };
  },
  getReleaseVersionDataContent(
    publicationSlug: string,
    releaseSlug: string,
  ): UseQueryOptions<ReleaseVersionDataContent> {
    return {
      queryKey: ['releaseVersionDataContent', publicationSlug, releaseSlug],
      queryFn: () =>
        publicationService.getReleaseVersionDataContent(
          publicationSlug,
          releaseSlug,
        ),
    };
  },
  getReleaseVersionRelatedInformation(
    publicationSlug: string,
    releaseSlug: string,
  ): UseQueryOptions<RelatedInformationItem[]> {
    return {
      queryKey: [
        'releaseVersionRelatedInformation',
        publicationSlug,
        releaseSlug,
      ],
      queryFn: () =>
        publicationService.getReleaseVersionRelatedInformation(
          publicationSlug,
          releaseSlug,
        ),
    };
  },
  getPreReleaseAccessList(
    publicationSlug: string,
    releaseSlug: string,
  ): UseQueryOptions<PreReleaseAccessListSummary> {
    return {
      queryKey: ['preReleaseAccessList', publicationSlug, releaseSlug],
      queryFn: () =>
        publicationService.getPreReleaseAccessList(
          publicationSlug,
          releaseSlug,
        ),
    };
  },
  getPublicationMethodologies(
    publicationSlug: string,
  ): UseQueryOptions<PublicationMethodologiesList> {
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
