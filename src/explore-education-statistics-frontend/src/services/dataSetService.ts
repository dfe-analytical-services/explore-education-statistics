import { PaginatedList } from '@common/services/types/pagination';
import { contentApi } from '@common/services/api';

export interface DataSetSummary {
  content: string;
  fileId: string;
  filename: string;
  fileSize: string;
  fileExtension: string;
  title: string;
  theme: {
    id: string;
    title: string;
  };
  publication: {
    id: string;
    title: string;
  };
  release: {
    id: string;
    title: string;
  };
  latestData: boolean;
  published: Date;
  timePeriods: {
    from?: string;
    to?: string;
  };
  geographicLevels: string[];
  filters: string[];
  indicators: string[];
}

export const dataSetOrderOptions = [
  'newest',
  'oldest',
  'relevance',
  'title',
] as const;

export type DataSetOrderOption = (typeof dataSetOrderOptions)[number];

export type DataSetSortParam = 'asc' | 'desc';

export type DataSetOrderParam = 'published' | 'title' | 'relevance';

export const dataSetFilters = [
  'latest',
  'publicationId',
  'releaseId',
  'searchTerm',
  'themeId',
] as const;

export type DataSetFilter = (typeof dataSetFilters)[number];

export interface DataSetListRequest {
  latestOnly?: 'true' | 'false';
  orderBy?: DataSetOrderParam;
  page?: number;
  pageSize?: number;
  publicationId?: string;
  releaseId?: string;
  searchTerm?: string;
  sort?: DataSetSortParam;
  themeId?: string;
}

const dataSetService = {
  listDataSets(
    params: DataSetListRequest,
  ): Promise<PaginatedList<DataSetSummary>> {
    return contentApi.get(`/data-sets`, {
      params,
    });
  },
};
export default dataSetService;
