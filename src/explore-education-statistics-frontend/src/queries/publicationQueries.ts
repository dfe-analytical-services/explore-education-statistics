import publicationService, {
  PublicationListSummary,
  PublicationTreeOptions,
  ReleaseSummary,
  Theme,
} from '@common/services/publicationService';
import createPublicationListRequest from '@frontend/modules/find-statistics/utils/createPublicationListRequest';
import { ParsedUrlQuery } from 'querystring';
import { UseQueryOptions } from '@tanstack/react-query';
import { PaginatedList } from '@common/services/types/pagination';
import { AzureKeyCredential, SearchClient } from '@azure/search-documents';

const ENDPOINT = 'https://s101d01-ees-srch.search.windows.net';
const INDEX = 'index-1';
const AZURE_SEARCH_QUERY_KEY = '';

const client = new SearchClient(
  ENDPOINT,
  INDEX,
  new AzureKeyCredential(AZURE_SEARCH_QUERY_KEY),
);

const publicationQueries = {
  getPublicationTree(query: PublicationTreeOptions): UseQueryOptions<Theme[]> {
    return {
      queryKey: ['publicationTree', query],
      queryFn: () => publicationService.getPublicationTree(query),
    };
  },
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
  listFromAzure(
    query: ParsedUrlQuery,
  ): UseQueryOptions<PaginatedList<PublicationListSummary>> {
    return {
      queryKey: ['listPublicationsFromAzure', query],
      queryFn: () =>
        publicationService.listPublications(
          createPublicationListRequest(query),
        ),
      // client.search(createAzSearchFromQuery(queryF))
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
