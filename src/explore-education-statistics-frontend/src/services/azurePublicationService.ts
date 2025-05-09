/* eslint-disable no-restricted-syntax */
import {
  PublicationListSummary,
  PublicationSortParam,
} from '@common/services/publicationService';
import { PaginatedList } from '@common/services/types/pagination';
import {
  AzureKeyCredential,
  odata,
  SearchClient,
} from '@azure/search-documents';
import { ReleaseType } from '@common/services/types/releaseType';
import { SortDirection } from '@common/services/types/sort';

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
  published: Date;
  summary: string;
  theme: string;
  title: string;
}

export interface AzurePublicationListRequest {
  page?: number;
  pageSize?: number;
  releaseType?: ReleaseType;
  search?: string;
  sort?: PublicationSortParam;
  sortDirection?: SortDirection;
  themeId?: string;
}

const azureSearchClient = new SearchClient<PublicationAzureSearchResult>(
  ENDPOINT,
  INDEX,
  new AzureKeyCredential(AZURE_SEARCH_QUERY_KEY),
);

const azurePublicationService = {
  async listPublications(
    params: AzurePublicationListRequest,
  ): Promise<PaginatedList<PublicationListSummary>> {
    const { releaseType, themeId, page, search } = params;

    let filter: string | undefined;
    if (releaseType && themeId) {
      filter = odata`releaseType eq ${releaseType} and theme eq ${themeId}`;
    } else if (releaseType) {
      filter = odata`releaseType eq ${releaseType}`;
    } else if (themeId) {
      // TODO amend theme to themeId once EES-6123 merged
      filter = odata`theme eq ${themeId}`;
    }
    const searchResults = await azureSearchClient.search(search || '', {
      // Pagination
      includeTotalCount: true,
      top: 10,
      skip: page && page > 1 ? (page - 1) * 10 : 0,
      queryType: 'semantic',

      // Semantic search
      semanticSearchOptions: {
        configurationName: 'semantic-configuration-1',
      },
      searchMode: 'any',
      scoringProfile: 'scoring-profile-1',
      highlightFields: 'content',

      // TODO Sorting
      // orderBy: ["title asc"],
      filter,
      // TODO amend theme to themeId once EES-6123 merged
      facets: ['theme,sort:count', 'releaseType'],
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
        page: params.page || 1,
        pageSize: 10,
      },
      results: [] as PublicationListSummary[],
    };

    for await (const result of results) {
      // console.log(result);
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
