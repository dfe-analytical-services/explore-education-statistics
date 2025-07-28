/* eslint-disable no-restricted-syntax */
import { env } from 'process';
import { PublicationListSummary } from '@common/services/publicationService';
import { PaginatedList } from '@common/services/types/pagination';
import { ReleaseType } from '@common/services/types/releaseType';
import { SortDirection } from '@common/services/types/sort';
import {
  AzureKeyCredential,
  FacetResult,
  odata,
  SearchClient,
  SearchOptions,
} from '@azure/search-documents';

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
  orderBy?: AzurePublicationOrderByParam;
  page?: number;
  pageSize?: number;
  releaseType?: string;
  search?: string;
  sortDirection?: SortDirection;
  themeId?: string;
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
    const {
      filter,
      orderBy,
      page = 1,
      pageSize = 10,
      releaseType,
      search = '',
      themeId,
    } = params;

    const searchOptionsBase = {
      includeTotalCount: true,
      orderBy: orderBy ? [orderBy] : undefined,
      queryType: !orderBy ? 'semantic' : undefined,
      searchMode: 'any',
      semanticSearchOptions: {
        configurationName: 'semantic-configuration-1',
      },
      scoringProfile: 'scoring-profile-1',
      skip: page > 1 ? (page - 1) * 10 : 0,
      top: 10,
    } as Pick<
      SearchOptions<AzurePublicationSearchResult>,
      | 'includeTotalCount'
      | 'orderBy'
      | ('queryType' & 'semanticSearchOptions')
      | 'scoringProfile'
      | 'searchMode'
      | 'skip'
      | 'top'
    >;

    // Get all search results
    const searchResults = await azureSearchClient.search(search, {
      ...searchOptionsBase,
      highlightFields: 'title,summary,content-3',
      facets: ['themeId,count:150,sort:count', 'releaseType'],
      filter,
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

    // If a theme filter is selected - let's get the facet counts
    // for as if we hadn't filtered by theme too
    const themeFacetResults = themeId
      ? await azureSearchClient.search(search, {
          ...searchOptionsBase,
          facets: ['themeId,count:150,sort:count'],
          select: [],
          // Filter still needs to account for releaseType if it is present
          filter: releaseType
            ? odata`releaseType eq ${releaseType}`
            : undefined,
        })
      : null;

    // If a releaseType filter is selected - let's get the facet counts
    // for as if we hadn't filtered by releaseType too
    const releaseTypeFacetResults = releaseType
      ? await azureSearchClient.search(search, {
          ...searchOptionsBase,
          facets: ['releaseType'],
          select: [],
          // Filter still needs to account for themeId if it is present
          filter: themeId ? odata`themeId eq ${themeId}` : undefined,
        })
      : null;

    // Now transform response into <PaginatedListWithAzureFacets<PublicationListSummary>>
    const { count = 0, results, facets = {} } = searchResults;
    const facetsCombined = {
      ...facets,
      ...themeFacetResults?.facets,
      ...releaseTypeFacetResults?.facets,
    };
    const publicationsResult = {
      paging: {
        totalPages: count === 0 ? 0 : Math.floor((count - 1) / pageSize) + 1,
        totalResults: count,
        page,
        pageSize,
      },
      results: [] as PublicationListSummary[],
      facets: facetsCombined,
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
            highlightPostTag: '</strong>',
            highlightPreTag: '<strong>',
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
