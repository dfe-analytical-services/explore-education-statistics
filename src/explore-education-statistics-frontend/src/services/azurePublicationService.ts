import { PublicationListSummary } from '@common/services/publicationService';
import { PaginatedList } from '@common/services/types/pagination';
import { SortDirection } from '@common/services/types/sort';
import { FacetResult } from '@azure/search-documents';
import { frontendApi } from '@common/services/api';

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

const azurePublicationService = {
  async listPublications(
    params: AzurePublicationListRequest,
  ): Promise<PaginatedListWithAzureFacets<PublicationListSummary>> {
    return frontendApi.post(`/search`, { searchOptions: params });
  },
  async suggestPublications(
    params: AzurePublicationListRequest,
  ): Promise<AzurePublicationSuggestResult[]> {
    return frontendApi.post(`/suggest`, { searchOptions: params });
  },
};

export default azurePublicationService;
