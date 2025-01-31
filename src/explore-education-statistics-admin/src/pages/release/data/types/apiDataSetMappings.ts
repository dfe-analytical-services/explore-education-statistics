import { MappingType } from '@admin/services/apiDataSetVersionService';
import {
  FilterOptionCandidateWithKey,
  FilterOptionMappingWithKey,
} from '@admin/pages/release/data/utils/getApiDataSetFilterMappings';
import {
  LocationCandidateWithKey,
  LocationMappingWithKey,
} from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import { LocationLevelKey } from '@common/utils/locationLevelsMap';

export interface PendingMappingUpdate {
  candidateKey?: string;
  groupKey: LocationLevelKey | string;
  sourceKey: string;
  type: MappingType;
  previousCandidate?: FilterOptionCandidateWithKey | LocationCandidateWithKey;
  previousMapping: FilterOptionMappingWithKey | LocationMappingWithKey;
}
