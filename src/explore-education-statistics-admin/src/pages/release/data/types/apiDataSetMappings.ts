import { Mapping, MappingType } from '@admin/services/apiDataSetVersionService';
import {
  FilterOptionCandidateWithKey,
  FilterOptionMappingWithKey,
} from '@admin/pages/release/data/utils/getApiDataSetFilterMappings';
import {
  LocationCandidateWithKey,
  LocationMappingWithKey,
} from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import { LocationLevelKey } from '@common/utils/locationLevelsMap';

export type AutoMapped<
  TMapping extends Mapping<unknown>,
  TCandidate,
> = TMapping & {
  candidate: TCandidate;
};

export type Mappable<
  TMapping extends Mapping<unknown>,
  TCandidate,
> = TMapping & {
  candidate?: TCandidate;
};

export type KeyedCandidate<T> = T & {
  key: string;
};

export interface PendingMappingUpdate {
  candidateKey?: string;
  groupKey: LocationLevelKey | string;
  sourceKey: string;
  type: MappingType;
  previousCandidate?: FilterOptionCandidateWithKey | LocationCandidateWithKey;
  previousMapping: FilterOptionMappingWithKey | LocationMappingWithKey;
}

export type PendingOptionMappingUpdate<
  TMapping extends Mapping<unknown>,
  TCandidate,
  TGroupKey extends string = string,
> = {
  groupKey: TGroupKey;
  type: MappingType;
  previousMapping: TMapping;
} & (
  | {
      type: 'ManualMapped';
      candidateKey: string;
      previousCandidate?: TCandidate;
    }
  | {
      type: 'ManualNone';
      previousCandidate: TCandidate;
    }
);
