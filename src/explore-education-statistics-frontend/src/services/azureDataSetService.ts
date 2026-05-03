import { SortDirection } from '@common/services/types/sort';
import { frontendApi } from '@common/services/api';
import { GeographicLevelCode } from '@common/utils/locationLevelsMap';
import { AzureOrderByParam } from '@frontend/services/azurePublicationService';
import {
  DataSetFileApi,
  DataSetFileSummary,
} from '@frontend/services/dataSetFileService';
import { PaginatedList } from '@common/services/types/pagination';

export interface AzureDataSetIndexItem {
  '@search.score': string;
  fileId: string;
  filename: string;
  fileExtension: string;
  fileSize: string;
  title: string;
  content: string;
  themeId: string;
  themeTitle: string;
  publicationId: string;
  publicationSlug: string;
  publicationTitle: string;
  releaseId: string;
  releaseTitle: string;
  releaseSlug: string;
  latestData: boolean;
  isSuperseded: boolean;
  published: string;
  lastUpdated: string;
  api: DataSetFileApi;
  numDataFileRows: number;
  geographicLevels: GeographicLevelCode[];
  geographicLevelsLabels: string[];
  indicators: string[];
  filters: string[];
  releaseType: string;
  timePeriodRange: {
    from: string;
    to: string;
  };
}

// export interface AzureDataSetSuggestResult {
//   highlightedMatch: string;
//   releaseSlug: string;
//   publicationSlug: string;
//   summary: string;
//   title: string;
// }

export interface AzureDataSetListRequest {
  filter?: string;
  orderBy?: AzureOrderByParam;
  page?: number;
  pageSize?: number;
  releaseType?: string;
  search?: string;
  sortDirection?: SortDirection;
  themeId?: string;
}

const azureDataSetService = {
  async listDataSets(
    params: AzureDataSetListRequest,
  ): Promise<PaginatedList<DataSetFileSummary>> {
    return frontendApi.post(`/search-datasets`, { searchOptions: params });
  },
  // async suggestPublications(
  //   params: AzureDataSetListRequest,
  // ): Promise<AzureDataSetSuggestResult[]> {
  //   return frontendApi.post(`/suggest-datasets`, { searchOptions: params });
  // },
};

export default azureDataSetService;
