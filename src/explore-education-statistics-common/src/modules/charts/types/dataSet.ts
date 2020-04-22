import { ChartSymbol, LineStyle } from '@common/modules/charts/types/chart';
import {
  CategoryFilter,
  Filter,
  Indicator,
  LocationCompositeId,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { Dictionary } from '@common/types';

export interface ChartData {
  name: string;
  [key: string]: number | string;
}

export interface DataSet {
  indicator: string;
  filters: string[];
  location?: LocationCompositeId;
  timePeriod?: string;
}

/**
 * Expanded variant of a data set where
 * we have all the filter details available.
 */
export interface ExpandedDataSet {
  indicator: Indicator;
  filters: CategoryFilter[];
  location?: LocationFilter;
  timePeriod?: TimePeriodFilter;
}

export interface DataSetCategory {
  filter: Filter;
  dataSets: Dictionary<{
    dataSet: DataSetConfiguration;
    value: number;
  }>;
}

/**
 * We want to try and remove this in the future
 * in favour of {@see DataSetConfiguration}.
 *
 * @deprecated
 */
export interface DeprecatedDataSetConfiguration {
  label: string;
  value: string;
  colour?: string;
  unit?: string;
  symbol?: ChartSymbol;
  lineStyle?: LineStyle;
}

export interface DataSetConfigurationOptions {
  label: string;
  colour?: string;
  unit?: string;
  symbol?: ChartSymbol;
  lineStyle?: LineStyle;
}

export interface DataSetConfiguration extends DataSet {
  config: DataSetConfigurationOptions;
}
