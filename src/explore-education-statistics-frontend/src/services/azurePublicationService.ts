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
      highlightFields: 'content',
      facets: ['themeId,count:60,sort:count', 'releaseType'],
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
    const { count, results, facets = {} } = searchResults;
    const publicationsResult = {
      paging: {
        totalPages: Math.floor((count || 0) / 10) + 1,
        totalResults: count || 0,
        page,
        pageSize: 10,
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
};

export default azurePublicationService;
