import {
  AutoMappedFilterOption,
  FilterOptionCandidateWithKey,
  FilterOptionMappingWithKey,
  MappableFilterOption,
} from '@admin/pages/release/data/utils/getApiDataSetFilterMappings';
import {
  AutoMappedLocation,
  LocationCandidateWithKey,
  LocationMappingWithKey,
  MappableLocation,
} from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import {
  AutoMappedIndicator,
  IndicatorCandidateWithKey,
  IndicatorMappingWithKey,
  MappableIndicator,
} from '@admin/pages/release/data/utils/getApiDataSetIndicatorMappings';
import {
  FilterOptionSource,
  IndicatorSource,
  LocationOptionSource,
} from '@admin/services/apiDataSetVersionService';

export type CandidateWithKey =
  | FilterOptionCandidateWithKey
  | LocationCandidateWithKey
  | IndicatorCandidateWithKey;

export type AutoMappedItem =
  | AutoMappedLocation
  | AutoMappedFilterOption
  | AutoMappedIndicator;

export type MappableSourceItem =
  | LocationOptionSource
  | FilterOptionSource
  | IndicatorSource;

export type MappableItem =
  | MappableLocation
  | MappableFilterOption
  | MappableIndicator;

export type MappingWithKey =
  | LocationMappingWithKey
  | FilterOptionMappingWithKey
  | IndicatorMappingWithKey;

export default {};
