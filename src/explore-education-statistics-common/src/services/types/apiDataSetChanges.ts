import {
  Filter,
  FilterOption,
  GeographicLevel,
  IndicatorOption,
  LocationGroup,
  LocationOption,
  TimePeriodOption,
} from '@common/services/types/apiDataSetMeta';

export interface SemVer {
  major: number;
  minor: number;
  patch: number;
}

export interface ApiDataSetVersionChanges {
  versionNumber: SemVer;
  majorChanges: ChangeSet;
  minorChanges: ChangeSet;
  notes: string;
  patchHistory: ApiDataSetVersionChanges[];
}

export interface ChangeSet {
  filters?: Change<Filter>[];
  filterOptions?: FilterOptionChanges[];
  geographicLevels?: Change<GeographicLevel>[];
  indicators?: Change<IndicatorOption>[];
  locationGroups?: Change<LocationGroup>[];
  locationOptions?: LocationOptionChanges[];
  timePeriods?: Change<TimePeriodOption>[];
}

export type Change<TState> = {
  previousState?: TState;
  currentState?: TState;
};

export interface FilterOptionChanges {
  filter: Filter;
  options: Change<FilterOption>[];
}

export interface LocationOptionChanges {
  level: GeographicLevel;
  options: Change<LocationOption>[];
}
