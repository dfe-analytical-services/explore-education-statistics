import client from '@admin/services/utils/service';
import {
  ApiDataSet,
  ApiDataSetLiveVersionSummary,
  ApiDataSetVersion,
} from '@admin/services/apiDataSetService';
import { PaginatedList } from '@common/services/types/pagination';
import { Dictionary } from '@common/types';
import { LocationLevelKey } from '@common/utils/locationLevelsMap';
import { ApiDataSetVersionChanges } from './types/apiDataSetChanges';

export type MappingType =
  | 'ManualMapped'
  | 'ManualNone'
  | 'AutoNone'
  | 'AutoMapped';

export type Mapping<TSource> = {
  candidateKey?: string;
  publicId: string;
  source: TSource;
  type: MappingType;
};

export interface FilterSource {
  label: string;
}

export interface FilterOptionSource {
  label: string;
}

export type FilterMapping = Mapping<FilterSource> & {
  optionMappings: Dictionary<FilterOptionMapping>;
};

export type FilterOptionMapping = Mapping<FilterOptionSource>;

export interface FilterCandidate {
  label: string;
  options: Dictionary<FilterSource>;
}

export interface FiltersMapping {
  candidates: Dictionary<FilterCandidate>;
  mappings: Dictionary<FilterMapping>;
}

export interface FilterMappingUpdate {
  sourceKey: string;
  candidateKey?: string;
  type: MappingType;
  filterKey: string;
}

interface FilterOptionsMappingUpdateRequest {
  updates: FilterMappingUpdate[];
}

interface FilterOptionsMappingUpdateResponse {
  updates: {
    filterKey: string;
    mapping: FilterMapping;
    sourceKey: string;
  }[];
}

export interface LocationOptionSource {
  label: string;
  code?: string;
  oldCode?: string;
  urn?: string;
  laEstab?: string;
  ukprn?: string;
}

export type LocationCandidate = LocationOptionSource;

export type LocationMapping = Mapping<LocationOptionSource>;

export interface LocationsMapping {
  levels: Dictionary<{
    candidates: Dictionary<LocationOptionSource>;
    mappings: Dictionary<LocationMapping>;
  }>;
}

export interface LocationMappingUpdate {
  candidateKey?: string;
  level: LocationLevelKey;
  sourceKey: string;
  type: MappingType;
}

interface LocationsMappingUpdateRequest {
  updates: LocationMappingUpdate[];
}

interface LocationsMappingUpdateResponse {
  updates: {
    level: LocationLevelKey;
    sourceKey: string;
    mapping: LocationMapping;
  }[];
}

export interface ListVersionsParams {
  dataSetId: string;
  page?: number;
  pageSize?: number;
}

const apiDataSetVersionService = {
  createVersion(data: {
    dataSetId: string;
    releaseFileId: string;
  }): Promise<ApiDataSet> {
    return client.post('/public-data/data-set-versions', data);
  },
  completeVersion(data: { dataSetVersionId: string }): Promise<ApiDataSet> {
    return client.post('/public-data/data-set-versions/complete', data);
  },
  deleteVersion(dataSetVersionId: string): Promise<void> {
    return client.delete(`/public-data/data-set-versions/${dataSetVersionId}`);
  },
  listVersions(
    params?: ListVersionsParams,
  ): Promise<PaginatedList<ApiDataSetLiveVersionSummary>> {
    return client.get(`/public-data/data-set-versions`, {
      params,
    });
  },
  getFiltersMapping(dataSetVersionId: string): Promise<FiltersMapping> {
    return client.get(
      `/public-data/data-set-versions/${dataSetVersionId}/mapping/filters`,
    );
  },
  updateFilterOptionsMapping(
    dataSetVersionId: string,
    data: FilterOptionsMappingUpdateRequest,
  ): Promise<FilterOptionsMappingUpdateResponse> {
    return client.patch(
      `/public-data/data-set-versions/${dataSetVersionId}/mapping/filters/options`,
      data,
    );
  },
  getLocationsMapping(dataSetVersionId: string): Promise<LocationsMapping> {
    return client.get(
      `/public-data/data-set-versions/${dataSetVersionId}/mapping/locations`,
    );
  },
  updateLocationsMapping(
    dataSetVersionId: string,
    data: LocationsMappingUpdateRequest,
  ): Promise<LocationsMappingUpdateResponse> {
    return client.patch(
      `/public-data/data-set-versions/${dataSetVersionId}/mapping/locations`,
      data,
    );
  },
  getChanges(dataSetVersionId: string): Promise<ApiDataSetVersionChanges> {
    return client.get(
      `/public-data/data-set-versions/${dataSetVersionId}/changes`,
    );
  },
  updateNotes(
    dataSetVersionId: string,
    data: {
      notes: string;
    },
  ): Promise<ApiDataSetVersion> {
    return client.patch(
      `/public-data/data-set-versions/${dataSetVersionId}`,
      data,
    );
  },
} as const;

export default apiDataSetVersionService;
