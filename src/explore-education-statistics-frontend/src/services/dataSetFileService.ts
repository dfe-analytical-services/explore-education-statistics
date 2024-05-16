import { PaginatedList } from '@common/services/types/pagination';
import { ReleaseType } from '@common/services/types/releaseType';
import { contentApi } from '@common/services/api';
import { SortDirection } from '@common/services/types/sort';

export interface DataSetFile {
  id: string;
  title: string;
  summary: string;
  file: { id: string; name: string; size: string; subjectId: string };
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
  api?: DataSetFileApi;
  meta: {
    geographicLevels: string[];
    timePeriod: {
      timeIdentifier: string;
      from: string;
      to: string;
    };
    filters: string[];
    indicators: string[];
  };
}

export interface DataSetFileSummary {
  id: string;
  fileId: string;
  filename: string;
  fileSize: string;
  fileExtension: string;
  title: string;
  content: string;
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
  api?: DataSetFileApi;
  meta: {
    geographicLevels: string[];
    timePeriod: {
      timeIdentifier: string;
      from: string;
      to: string;
    };
    filters: string[];
    indicators: string[];
  };
}

export interface DataSetFileApi {
  id: string;
  version: string;
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
  'dataSetType',
  'latest',
  'publicationId',
  'releaseId',
  'searchTerm',
  'themeId',
] as const;

export type DataSetFileFilter = (typeof dataSetFileFilters)[number];

export type DataSetType = 'all' | 'api';

export interface DataSetFileListRequest {
  dataSetType?: DataSetType;
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
    return contentApi.get(`/data-set-files/${dataSetId}`);
  },
};
export default dataSetFileService;
