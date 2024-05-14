import { publicApi } from '@common/services/api';
import { PaginatedList } from '@common/services/types/pagination';

export type ApiDataSetType = 'Major' | 'Minor';
export type ApiDataSetStatus = 'Published' | 'Deprecated' | 'Withdrawn';
export type ApiGeographicLevels =
  | 'EDA'
  | 'INST'
  | 'LA'
  | 'LAD'
  | 'LEP'
  | 'LSIP'
  | 'MAT'
  | 'MCA'
  | 'NAT'
  | 'OA'
  | 'PA'
  | 'PCON'
  | 'PROV'
  | 'REG'
  | 'RSC'
  | 'SCH'
  | 'SPON'
  | 'WARD';

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
    geographicLevels: ApiGeographicLevels[];
    filters: string[];
    indicators: string[];
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
  timePeriods: {
    start: string;
    end: string;
  };
  geographicLevels: ApiGeographicLevels[];
  filters: string[];
  indicators: string[];
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
};
export default apiDataSetService;
