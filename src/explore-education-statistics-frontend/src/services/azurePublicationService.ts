/* eslint-disable no-restricted-syntax */
import { env } from 'process';
import { PublicationListSummary } from '@common/services/publicationService';
import { PaginatedList } from '@common/services/types/pagination';
import { AzureKeyCredential, SearchClient } from '@azure/search-documents';
import { ReleaseType } from '@common/services/types/releaseType';
import { SortDirection } from '@common/services/types/sort';

export interface PublicationAzureSearchResult {
  content: string;
  releaseSlug: string;
  releaseType: string;
  releaseVersionId: string;
  publicationSlug: string;
  published: Date;
  summary: string;
  theme: string;
  title: string;
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

const azureSearchClient = new SearchClient<PublicationAzureSearchResult>(
  AZURE_SEARCH_ENDPOINT || '',
  AZURE_SEARCH_INDEX || '',
  new AzureKeyCredential(AZURE_SEARCH_QUERY_KEY || ''),
);

const azurePublicationService = {
  async listPublications(
    params: AzurePublicationListRequest,
  ): Promise<PaginatedList<PublicationListSummary>> {
    const { filter, orderBy, page = 1, search } = params;

    const searchResults = await azureSearchClient.search(search || '', {
      // Pagination
      includeTotalCount: true,
      top: 10,
      skip: page > 1 ? (page - 1) * 10 : 0,

      // Semantic search
      // queryType: 'semantic',
      semanticSearchOptions: {
        configurationName: 'semantic-configuration-1',
      },
      searchMode: 'any',
      scoringProfile: 'scoring-profile-1',
      highlightFields: 'content',

      // TODO amend theme to themeId once EES-6123 merged
      facets: ['theme,sort:count', 'releaseType'],
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
        'theme',
        'title',
      ],
    });

    // Transform response into <PaginatedList<PublicationListSummary>>
    const { count, results } = searchResults;
    const publicationsResult = {
      paging: {
        totalPages: Math.floor((count || 0) / 10) + 1,
        totalResults: count || 0,
        page,
        pageSize: 10,
      },
      results: [] as PublicationListSummary[],
    };

    for await (const result of results) {
      const { document } = result;
      const {
        theme,
        title,
        summary,
        published,
        releaseVersionId: id,
        releaseSlug: slug,
        releaseType: type,
      } = document;

      publicationsResult.results.push({
        theme,
        title,
        summary,
        highlightContent: result.highlights?.content?.join(' ... ') || null,
        published: published.toString(),
        id,
        rank: result.score,
        slug,
        latestReleaseSlug: slug,
        type: type as ReleaseType,
      });
    }

    return publicationsResult;
  },
};

export default azurePublicationService;
