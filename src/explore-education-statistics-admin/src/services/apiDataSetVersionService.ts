import client from '@admin/services/utils/service';
import { ApiDataSet } from '@admin/services/apiDataSetService';
import { Dictionary } from '@common/types';
import { LocationLevelsType } from '@common/utils/locationLevelsMap';

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
  type: MappingType;
  source: {
    label: string;
    code?: string;
    oldCode?: string;
    urn?: string;
    laEstab?: string;
    ukprn?: string;
  };
}

export interface LocationsMapping {
  levels: {
    [K in LocationLevelsType]: {
      candidates: Dictionary<LocationCandidate>;
      mappings: Dictionary<LocationMapping>;
    };
  };
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
} as const;

export default apiDataSetVersionService;
