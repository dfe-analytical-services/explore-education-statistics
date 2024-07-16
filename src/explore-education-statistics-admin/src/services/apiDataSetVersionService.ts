import client from '@admin/services/utils/service';
import { ApiDataSet } from '@admin/services/apiDataSetService';
import { Dictionary } from '@common/types';
import { LocationLevelKey } from '@common/utils/locationLevelsMap';

export type MappingType =
  | 'ManualMapped'
  | 'ManualNone'
  | 'AutoNone'
  | 'AutoMapped';

export interface LocationCandidate {
  label: string;
  code?: string;
  oldCode?: string;
  urn?: string;
  laEstab?: string;
  ukprn?: string;
}

export interface LocationMapping {
  candidateKey?: string;
  publicId: string;
  type: MappingType;
  source: LocationCandidate;
}

export interface LocationsMapping {
  levels: Partial<
    Record<
      LocationLevelKey,
      {
        candidates: Dictionary<LocationCandidate>;
        mappings: Dictionary<LocationMapping>;
      }
    >
  >;
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
