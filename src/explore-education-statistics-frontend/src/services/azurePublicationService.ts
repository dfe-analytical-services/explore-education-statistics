/* eslint-disable no-restricted-syntax */
import { env } from 'process';
import { PublicationListSummary } from '@common/services/publicationService';
import { PaginatedList } from '@common/services/types/pagination';
import {
  AzureKeyCredential,
  FacetResult,
  SearchClient,
} from '@azure/search-documents';
import { ReleaseType } from '@common/services/types/releaseType';
import { SortDirection } from '@common/services/types/sort';

export interface AzurePublicationSearchResult {
  '@search.score': string;
  content: string;
  releaseSlug: string;
  releaseType: string;
  releaseVersionId: string;
  publicationId: string;
  publicationSlug: string;
  published: Date;
  releaseId: string;
  summary: string;
  themeId: string;
  themeTitle: string;
  title: string;
  typeBoost: string;
  metadata_storage_last_modified: Date;
  metadata_storage_name: string;
}

export interface AzurePublicationSuggestResult {
  highlightedMatch: string;
  releaseSlug: string;
  publicationSlug: string;
  summary: string;
  title: string;
}

export interface PaginatedListWithAzureFacets<T> extends PaginatedList<T> {
  facets: {
    [propertyName: string]: FacetResult[];
  };
}

export type AzurePublicationOrderByParam =
  | 'published asc'
  | 'published desc'
  | 'title asc'
  | undefined;

export interface AzurePublicationListRequest {
  filter?: string;
  page?: number;
  pageSize?: number;
  search?: string;
  orderBy?: AzurePublicationOrderByParam;
  sortDirection?: SortDirection;
}

const { AZURE_SEARCH_ENDPOINT, AZURE_SEARCH_INDEX, AZURE_SEARCH_QUERY_KEY } =
  env;

const azureSearchClient = new SearchClient<AzurePublicationSearchResult>(
  AZURE_SEARCH_ENDPOINT || '',
  AZURE_SEARCH_INDEX || '',
  new AzureKeyCredential(AZURE_SEARCH_QUERY_KEY || ''),
);

const azurePublicationService = {
  async listPublications(
    params: AzurePublicationListRequest,
  ): Promise<PaginatedListWithAzureFacets<PublicationListSummary>> {
    const { filter, orderBy, page = 1, search } = params;

    const searchResults = await azureSearchClient.search(search || '', {
      includeTotalCount: true,
      top: 10,
      skip: page > 1 ? (page - 1) * 10 : 0,
      queryType: !orderBy ? 'semantic' : undefined,
      semanticSearchOptions: {
        configurationName: 'semantic-configuration-1',
      },
      searchMode: 'any',
      scoringProfile: 'scoring-profile-1',
      highlightFields: 'title,summary,content-3',
      facets: ['themeId,count:150,sort:count', 'releaseType'],
      filter,
      orderBy: orderBy ? [orderBy] : undefined,
      select: [
        'content',
        'releaseSlug',
        'releaseType',
        'releaseVersionId',
        'publicationSlug',
        'published',
        'summary',
        'themeTitle',
        'title',
      ],
    });

    // Transform response into <PaginatedListWithAzureFacets<PublicationListSummary>>
    const { count = 0, results, facets = {} } = searchResults;
    const pageSize = 10;
    const publicationsResult = {
      paging: {
        totalPages: count === 0 ? 0 : Math.floor((count - 1) / pageSize) + 1,
        totalResults: count,
        page,
        pageSize,
      },
      results: [] as PublicationListSummary[],
      facets,
    };

    for await (const result of results) {
      const { document } = result;
      const {
        themeTitle,
        title,
        summary,
        publicationSlug: slug,
        published,
        releaseVersionId: id,
        releaseSlug: latestReleaseSlug,
        releaseType: type,
      } = document;

      publicationsResult.results.push({
        theme: themeTitle,
        title,
        summary,
        highlightContent: result.highlights?.content?.join(' ... ') || null,
        highlightSummary: result.highlights?.summary?.join(' ... ') || null,
        highlightTitle: result.highlights?.title?.join(' ... ') || null,
        published: published.toString(),
        id,
        rank: result.score,
        slug,
        latestReleaseSlug,
        type: type as ReleaseType,
      });
    }

    return publicationsResult;
  },
  async suggestPublications(
    params: AzurePublicationListRequest,
  ): Promise<AzurePublicationSuggestResult[]> {
    const { filter, search = '' } = params;

    const suggestResults =
      search?.length > 2
        ? await azureSearchClient.suggest(search, 'suggester-1', {
            select: ['releaseSlug', 'title', 'summary', 'publicationSlug'],
            filter,
            searchFields: ['title', 'summary'],
            useFuzzyMatching: true,
            top: 3,
            highlightPostTag: '</mark>',
            highlightPreTag: '<mark>',
          })
        : null;

    let resultsToReturn = [] as AzurePublicationSuggestResult[];
    if (suggestResults?.results) {
      resultsToReturn = suggestResults?.results.map(result => {
        return {
          ...result.document,
          highlightedMatch: result.text,
        } as AzurePublicationSuggestResult;
      });
    }
    return resultsToReturn;
  },
};

export default azurePublicationService;
