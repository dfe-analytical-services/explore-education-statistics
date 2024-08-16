import client from '@admin/services/utils/service';
import { ApiDataSet } from '@admin/services/apiDataSetService';
import { Dictionary } from '@common/types';
import { LocationLevelKey } from '@common/utils/locationLevelsMap';

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

export interface LocationCandidate {
  label: string;
  code?: string;
  oldCode?: string;
  urn?: string;
  laEstab?: string;
  ukprn?: string;
}

export type LocationMapping = Mapping<LocationCandidate>;

export interface LocationsMapping {
  levels: Dictionary<{
    candidates: Dictionary<LocationCandidate>;
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

const apiDataSetVersionService = {
  createVersion(data: {
    dataSetId: string;
    releaseFileId: string;
  }): Promise<ApiDataSet> {
    return client.post('/public-data/data-set-versions', data);
  },
  deleteVersion(versionId: string): Promise<void> {
    return client.delete(`/public-data/data-set-versions/${versionId}`);
  },
  getFiltersMapping(versionId: string): Promise<FiltersMapping> {
    return client.get(
      `/public-data/data-set-versions/${versionId}/mapping/filters`,
    );
  },
  updateFilterOptionsMapping(
    versionId: string,
    data: FilterOptionsMappingUpdateRequest,
  ): Promise<FilterOptionsMappingUpdateResponse> {
    return client.patch(
      `/public-data/data-set-versions/${versionId}/mapping/filters/options`,
      data,
    );
  },
  getLocationsMapping(versionId: string): Promise<LocationsMapping> {
    return client.get(
      `/public-data/data-set-versions/${versionId}/mapping/locations`,
    );
  },
  updateLocationsMapping(
    versionId: string,
    data: LocationsMappingUpdateRequest,
  ): Promise<LocationsMappingUpdateResponse> {
    return client.patch(
      `/public-data/data-set-versions/${versionId}/mapping/locations`,
      data,
    );
  },
} as const;

export default apiDataSetVersionService;
