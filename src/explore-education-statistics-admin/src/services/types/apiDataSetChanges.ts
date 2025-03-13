import {
  Filter,
  FilterOption,
  GeographicLevel,
  IndicatorOption,
  LocationGroup,
  LocationOption,
  TimePeriodOption,
} from '@common/services/types/apiDataSetMeta';
import { IdTitlePair } from './common';
import { DataSetVersionStatus, DataSetVersionType } from '../apiDataSetService';

export interface ApiDataSetVersionChanges {
  dataSet: IdTitlePair;
  dataSetVersion: DataSetVersionViewModel2;
  changes: ApiDataSetVersionChanges2;
}

export interface DataSetVersionViewModel2 {
  id: string;
  version: string;
  status: DataSetVersionStatus;
  type: DataSetVersionType;
  notes: string;
}

export interface ApiDataSetVersionChanges2 {
  majorChanges: ChangeSet;
  minorChanges: ChangeSet;
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
