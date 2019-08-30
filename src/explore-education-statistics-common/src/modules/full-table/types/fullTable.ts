import { Dictionary } from '@common/types';
import { GroupedFilterOptions } from '@common/modules/full-table/services/tableBuilderService';
import {
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/full-table/types/filters';

export interface FullTableMeta {
  publicationName: string;
  subjectId: string;
  subjectName: string;
  locations: LocationFilter[];
  timePeriodRange: TimePeriodFilter[];
  filters: Dictionary<{
    legend: string;
    hint?: string;
    options: GroupedFilterOptions;
    totalValue?: string;
  }>;
  indicators: Indicator[];
  footnotes: {
    id: number;
    label: string;
  }[];
}

export interface TableData {
  subjectMeta: FullTableMeta;
  results: {
    timePeriod: string;
    measures: Dictionary<string>;
    filters: string[];
    location: Dictionary<{
      code: string;
      name: string;
    }>;
  }[];
}

export interface FullTable extends TableData {
  title?: string;
}
