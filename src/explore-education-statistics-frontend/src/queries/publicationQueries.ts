/* eslint-disable no-restricted-syntax */

import publicationService, {
  PublicationListSummary,
  PublicationTreeOptions,
  ReleaseSummary,
  Theme,
} from '@common/services/publicationService';
import createPublicationListRequest, {
  getParamsFromQuery,
} from '@frontend/modules/find-statistics/utils/createPublicationListRequest';
import { ParsedUrlQuery } from 'querystring';
import { UseQueryOptions } from '@tanstack/react-query';
import { PaginatedList } from '@common/services/types/pagination';
import {
  AzureKeyCredential,
  SearchClient,
  SearchDocumentsResult,
  SearchResult,
} from '@azure/search-documents';

// TODO change to env config values
const ENDPOINT = 'https://s101d01-ees-srch.search.windows.net';
const INDEX = 'index-1';
const AZURE_SEARCH_QUERY_KEY = '';

export interface PublicationAzureSearchResult {
  content: string;
  releaseSlug: string;
  releaseType: string;
  releaseVersionId: string;
  publicationSlug: string;
  published: string;
  summary: string;
  theme: string;
  title: string;
}

const azureSearchClient = new SearchClient<PublicationAzureSearchResult>(
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
  // list(
  //   query: ParsedUrlQuery,
  // ): UseQueryOptions<
  //   PaginatedList<PublicationListSummary | PublicationAzureSearchResult>
  // > {
  //   return {
  //     queryKey: ['listPublications', query],
  //     queryFn: () => {
  //       if (query.azsearch && query.azsearch === 'true') {
  //           // We need to map <FindStatisticsPageQuery> to azclient SearchOptions
  //           const params = getParamsFromQuery(query);
  //           const searchResults = azureSearchClient.search(
  //             params.search || '',
  //             {
  //               includeTotalCount: true,
  //               top: 10,
  //               skip: params.page ? params.page - 1 * 10 : 0,
  //             },
  //           );
  //           console.log({ searchResults });
  //           // And then map results back to <PaginatedList<PublicationListSummary>>
  //           return {
  //             paging: {
  //               totalPages: 1,
  //               totalResults: 0,
  //               page: 1,
  //               pageSize: 10,
  //             },
  //             results: searchResults,
  //           };
  //       } else {
  //         const oldServiceResults = publicationService.listPublications(
  //           createPublicationListRequest(query),
  //         );
  //         console.log({ oldServiceResults });

  //         return oldServiceResults;
  //       }
  //     },
  //   };
  // },
  listAzure(
    query: ParsedUrlQuery,
  ): UseQueryOptions<
    PaginatedList<SearchResult<PublicationAzureSearchResult>>
  > {
    return {
      queryKey: ['listPublicationsAzure', query],
      queryFn: async () => {
        // We need to map <FindStatisticsPageQuery> to azclient SearchOptions
        // TODO add in faceting and sorting
        const params = getParamsFromQuery(query);
        const searchResults = await azureSearchClient.search(
          params.search || '',
          {
            includeTotalCount: true,
            top: 10,
            // TODO Check if params.page needs number check or safe to assume type
            skip: params.page && params.page > 1 ? (params.page - 1) * 10 : 0,
          },
        );
        // return searchResults;

        const publicationsResult = {
          paging: {
            totalPages: Math.floor((searchResults.count || 0) / 10) + 1,
            totalResults: searchResults.count || 0,
            page: params.page || 1,
            pageSize: 10,
          },
          results: [] as SearchResult<PublicationAzureSearchResult>[],
        };

        for await (const result of searchResults.results) {
          publicationsResult.results.push(result);
        }

        return publicationsResult;
      },
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
