import { MappingType } from '@admin/services/apiDataSetVersionService';
import { LocationLevelKey } from '@common/utils/locationLevelsMap';
import {
  CandidateWithKey,
  MappingWithKey,
} from '@admin/pages/release/data/utils/mappingTypes';

export interface PendingMappingUpdate {
  candidateKey?: string;
  groupKey?: LocationLevelKey | string;
  sourceKey: string;
  type: MappingType;
  previousCandidate?: CandidateWithKey;
  previousMapping: MappingWithKey;
}

export interface PendingGroupableMappingUpdate extends PendingMappingUpdate {
  groupKey: LocationLevelKey | string;
}
