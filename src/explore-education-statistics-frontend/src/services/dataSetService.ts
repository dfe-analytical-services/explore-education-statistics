import { PaginatedList } from '@common/services/types/pagination';
import { ReleaseType } from '@common/services/types/releaseType';
import { contentApi } from '@common/services/api';
import { SortDirection } from '@common/services/types/sort';

export interface DataSet {
  id: string;
  file: { id: string; name: string; size: string };
  release: {
    id: string;
    isLatestPublishedRelease: boolean;
    publication: {
      id: string;
      slug: string;
      themeTitle: string;
      title: string;
    };
    published: Date;
    slug: string;
    title: string;
    type: ReleaseType;
  };
  summary: string;
  title: string;
  // These aren't in the backend yet, so may change.
  filters: string[];
  geographicLevels: string[];
  indicators: string[];
  timePeriods: {
    from?: string;
    to?: string;
  };
}

export interface DataSetSummary {
  id: string;
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
  // These aren't in the backend yet, so may change.
  timePeriods: {
    from?: string;
    to?: string;
  };
  geographicLevels: string[];
  filters: string[];
  indicators: string[];
}

export const dataSetSortOptions = [
  'newest',
  'oldest',
  'relevance',
  'title',
] as const;

export type DataSetSortOption = (typeof dataSetSortOptions)[number];

export type DataSetSortParam = 'published' | 'title' | 'relevance';

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
  page?: number;
  pageSize?: number;
  publicationId?: string;
  releaseId?: string;
  searchTerm?: string;
  sort?: DataSetSortParam;
  sortDirection?: SortDirection;
  themeId?: string;
}

const dataSetService = {
  getDataSet(dataSetId: string): Promise<DataSet> {
    return contentApi.get(`/data-set/${dataSetId}`);
  },
  listDataSets(
    params: DataSetListRequest,
  ): Promise<PaginatedList<DataSetSummary>> {
    return contentApi.get(`/data-sets`, {
      params,
    });
  },
};
export default dataSetService;
