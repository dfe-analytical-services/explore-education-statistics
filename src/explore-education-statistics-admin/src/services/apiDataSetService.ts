import { IdTitlePair } from '@admin/services/types/common';
import client from '@admin/services/utils/service';
import { PaginatedList } from '@common/services/types/pagination';

export interface ApiDataSetSummary {
  id: string;
  title: string;
  summary: string;
  status: DataSetStatus;
  supersedingDataSetId?: string;
  draftVersion?: ApiDataSetDraftVersionSummary;
  latestLiveVersion?: ApiDataSetLiveVersionSummary;
}

export interface ApiDataSetVersionSummary {
  id: string;
  version: string;
  type: DataSetVersionType;
}

export interface ApiDataSetDraftVersionSummary
  extends ApiDataSetVersionSummary {
  status: DataSetDraftVersionStatus;
}

export interface ApiDataSetLiveVersionSummary extends ApiDataSetVersionSummary {
  published: string;
  status: DataSetLiveVersionStatus;
}

export interface ApiDataSet {
  id: string;
  title: string;
  summary: string;
  status: DataSetStatus;
  supersedingDataSetId?: string;
  draftVersion?: ApiDataSetDraftVersion;
  latestLiveVersion?: ApiDataSetLiveVersion;
}

export interface ApiDataSetVersion {
  id: string;
  version: string;
  status: DataSetVersionStatus;
  type: DataSetVersionType;
  dataSetFileId: string;
  releaseVersion: IdTitlePair;
  totalResults: number;
}

export interface ApiDataSetDraftVersion extends ApiDataSetVersion {
  status: DataSetDraftVersionStatus;
  timePeriods?: TimePeriodRange;
  geographicLevels?: string[];
  filters?: string[];
  indicators?: string[];
}

export interface ApiDataSetLiveVersion extends ApiDataSetVersion {
  status: DataSetLiveVersionStatus;
  published: string;
  timePeriods: TimePeriodRange;
  geographicLevels: string[];
  filters: string[];
  indicators: string[];
}

export interface TimePeriodRange {
  start: string;
  end: string;
}

export type DataSetStatus = 'Draft' | 'Published' | 'Deprecated' | 'Withdrawn';

export type DataSetDraftVersionStatus =
  | 'Processing'
  | 'Failed'
  | 'Mapping'
  | 'Draft'
  | 'Cancelled';

export type DataSetLiveVersionStatus = 'Published' | 'Deprecated' | 'Withdrawn';

export type DataSetVersionStatus =
  | DataSetDraftVersionStatus
  | DataSetLiveVersionStatus;

export type DataSetVersionType = 'Major' | 'Minor';

const apiDataSetService = {
  async listDataSets(publicationId: string): Promise<ApiDataSetSummary[]> {
    const { results } = await client.get<PaginatedList<ApiDataSetSummary>>(
      '/public-data/data-sets',
      {
        params: {
          publicationId,
          pageSize: 100,
        },
      },
    );

    return results;
  },
  createDataSet(data: { releaseFileId: string }): Promise<ApiDataSet> {
    return client.post('/public-data/data-sets', data);
  },
};

export default apiDataSetService;
