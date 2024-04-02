import { PaginatedList } from '@common/services/types/pagination';
import { ReleaseType } from '@common/services/types/releaseType';
import { contentApi } from '@common/services/api';
import { SortDirection } from '@common/services/types/sort';

export interface DataSetFile {
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

export interface DataSetFileSummary {
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

export const dataSetFileSortOptions = [
  'newest',
  'oldest',
  'relevance',
  'title',
] as const;

export type DataSetFileSortOption = (typeof dataSetFileSortOptions)[number];

export type DataSetFileSortParam = 'published' | 'title' | 'relevance';

export const dataSetFileFilters = [
  'latest',
  'publicationId',
  'releaseId',
  'searchTerm',
  'themeId',
] as const;

export type DataSetFileFilter = (typeof dataSetFileFilters)[number];

export interface DataSetFileListRequest {
  latestOnly?: 'true' | 'false';
  page?: number;
  pageSize?: number;
  publicationId?: string;
  releaseId?: string;
  searchTerm?: string;
  sort?: DataSetFileSortParam;
  sortDirection?: SortDirection;
  themeId?: string;
}

const dataSetFileService = {
  listDataSetFiles(
    params: DataSetFileListRequest,
  ): Promise<PaginatedList<DataSetFileSummary>> {
    return contentApi.get(`/data-set-files`, {
      params,
    });
  },
  getDataSetFile(dataSetId: string): Promise<DataSetFile> {
    return contentApi.get(`/data-set-file/${dataSetId}`);
  },
};
export default dataSetFileService;
