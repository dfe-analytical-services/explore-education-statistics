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
  order?: number;
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
 * Don't use this for storing configuration.
 * @deprecated use {@see LegendItem} instead.
 */
export interface DataSetConfigurationOptions {
  label: string;
  colour: string;
  symbol?: ChartSymbol;
  lineStyle?: LineStyle;
}

/**
 * Don't use this for storing configuration.
 * @deprecated use {@see LegendItem} instead.
 */
export interface DataSetConfiguration extends DataSet {
  config?: DataSetConfigurationOptions;
}
