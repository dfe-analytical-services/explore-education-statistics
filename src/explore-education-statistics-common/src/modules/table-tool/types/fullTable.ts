import { GroupedFilterOptions } from '@common/modules/table-tool/services/tableBuilderService';
import {
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { Dictionary } from '@common/types';

export interface FullTableMeta {
  publicationName: string;
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
    id: string;
    label: string;
  }[];
}

export interface FullTable {
  title?: string;
  subjectMeta: FullTableMeta;
  results: {
    filters: string[];
    geographicLevel: string;
    location: Dictionary<{
      code: string;
      name: string;
    }>;
    measures: Dictionary<string>;
    timePeriod: string;
  }[];
}
