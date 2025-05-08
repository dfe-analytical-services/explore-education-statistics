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
    const searchResults = await azureSearchClient.search(params.search || '', {
      includeTotalCount: true,
      top: 10,
      // TODO Check if params.page needs number check or safe to assume type
      skip: params.page && params.page > 1 ? (params.page - 1) * 10 : 0,
      queryType: 'semantic',
      semanticSearchOptions: {
        configurationName: 'semantic-configuration-1',
      },
      // orderBy: ["title asc"],
      filter: params.themeId ? odata`theme eq ${params.themeId}` : undefined,
      facets: ['theme,sort:count', 'releaseType'],
      highlightFields: 'summary',
      searchMode: 'any',
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
      scoringProfile: 'scoring-profile-1',
    });
    // return searchResults;
    // console.log(searchResults);

    const publicationsResult = {
      paging: {
        totalPages: Math.floor((searchResults.count || 0) / 10) + 1,
        totalResults: searchResults.count || 0,
        page: params.page || 1,
        pageSize: 10,
      },
      results: [] as PublicationListSummary[],
    };
    console.log(searchResults);

    for await (const result of searchResults.results) {
      console.log(result);
      const { document } = result;
      publicationsResult.results.push({
        theme: document.theme,
        title: document.title,
        summary:
          result.highlights?.summary?.reduce(
            (highlight, fullstring) => `${fullstring} ${highlight}`,
            '',
          ) || document.summary,
        published: document.published.toString(),
        id: document.releaseVersionId,
        rank: result.score,
        slug: document.releaseSlug,
        latestReleaseSlug: document.releaseSlug,
        type: document.releaseType as ReleaseType,
      });
    }

    return publicationsResult;
  },
};
export default azurePublicationService;
