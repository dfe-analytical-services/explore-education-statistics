import { PaginatedList } from '@common/services/types/pagination';
import { ReleaseType } from '@common/services/types/releaseType';
import { contentApi } from '@common/services/api';
import { SortDirection } from '@common/services/types/sort';
import { GeographicLevelCode } from '@common/utils/locationLevelsMap';

export interface DataSetVariable {
  label: string;
  value: string;
}

export interface DataSetCsvPreview {
  headers: string[];
  rows: string[][];
}

export interface DataSetFootnote {
  id: string;
  label: string;
}

export interface DataSetFile {
  id: string;
  title: string;
  summary: string;
  file: {
    id: string;
    name: string;
    size: string;
    meta: {
      numDataFileRows: number;
      geographicLevels: string[];
      timePeriodRange: {
        from: string;
        to: string;
      };
      filters: string[];
      indicators: string[];
    };
    dataCsvPreview: DataSetCsvPreview;
    variables: DataSetVariable[];
    subjectId: string;
  };
  release: {
    id: string;
    isLatestPublishedRelease: boolean;
    isSuperseded: boolean;
    publication: {
      id: string;
      slug: string;
      themeTitle: string;
      title: string;
    };
    published: Date;
    lastUpdated: string;
    slug: string;
    title: string;
    type: ReleaseType;
  };
  footnotes: DataSetFootnote[];
  api?: DataSetFileApi;
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
    slug: string;
  };
  release: {
    id: string;
    title: string;
    slug: string;
  };
  latestData: boolean;
  isSuperseded: boolean;
  published: Date;
  lastUpdated: string;
  api?: DataSetFileApi;
  meta: {
    numDataFileRows: number;
    geographicLevels: string[];
    timePeriodRange: {
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

export type DataSetFileSortParam =
  | 'published'
  | 'title'
  | 'relevance'
  | 'natural';

export type DataSetType = 'all' | 'api';

export interface DataSetFileListRequest {
  dataSetType?: DataSetType;
  latestOnly?: 'true' | 'false';
  page?: number;
  pageSize?: number;
  publicationId?: string;
  releaseId?: string;
  geographicLevel?: GeographicLevelCode;
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
  getDataSetFile(dataSetFileId: string): Promise<DataSetFile> {
    return contentApi.get(`/data-set-files/${dataSetFileId}`);
  },
};
export default dataSetFileService;
