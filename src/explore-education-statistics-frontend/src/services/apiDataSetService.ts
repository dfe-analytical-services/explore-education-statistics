import { publicApi } from '@common/services/api';
import { ApiDataSetVersionChanges } from '@common/services/types/apiDataSetChanges';
import { PaginatedList } from '@common/services/types/pagination';

export type ApiDataSetType = 'Major' | 'Minor';
export type ApiDataSetStatus = 'Published' | 'Deprecated' | 'Withdrawn';

export interface ApiDataSet {
  id: string;
  title: string;
  summary: string;
  status: ApiDataSetStatus;
  latestVersion: {
    version: string;
    published: string;
    totalResults: number;
    timePeriods: {
      start: string;
      end: string;
    };
    geographicLevels: string[];
    filters: string[];
    indicators: string[];
    file: {
      id: string;
    };
  };
}

export interface ApiDataSetVersion {
  version: string;
  type: ApiDataSetType;
  status: ApiDataSetStatus;
  published: string;
  withdrawn?: Date;
  notes: string;
  totalResults: number;
  file: ApiDataSetVersionFile;
  release: ApiDataSetVersionRelease;
  timePeriods: {
    start: string;
    end: string;
  };
  geographicLevels: string[];
  filters: string[];
  indicators: string[];
}

export interface ApiDataSetVersionFile {
  id: string;
}

export interface ApiDataSetVersionRelease {
  title: string;
  slug: string;
}

export interface ApiDataSetVersionsListRequest {
  page?: number;
  pageSize?: number;
}

const apiDataSetService = {
  getDataSet(dataSetId: string): Promise<ApiDataSet> {
    return publicApi.get(`/data-sets/${dataSetId}`);
  },
  getDataSetVersion(
    dataSetId: string,
    dataSetVersion: string,
  ): Promise<ApiDataSetVersion> {
    return publicApi.get(`/data-sets/${dataSetId}/versions/${dataSetVersion}`);
  },
  listDataSetVersions(
    dataSetId: string,
    params?: ApiDataSetVersionsListRequest,
  ): Promise<PaginatedList<ApiDataSetVersion>> {
    return publicApi.get(`/data-sets/${dataSetId}/versions`, { params });
  },
  getDataSetVersionChanges(
    dataSetId: string,
    dataSetVersion: string,
    showPatchHistory: boolean = false,
  ): Promise<ApiDataSetVersionChanges> {
    return publicApi.get(
      `/data-sets/${dataSetId}/versions/${dataSetVersion}/changes`,
      { params: { includePatchHistory: showPatchHistory } },
    );
  },
};

export default apiDataSetService;
