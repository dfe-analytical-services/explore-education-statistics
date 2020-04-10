import {
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import {
  GroupedFilterOptions,
  TableDataResult,
} from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';

export interface FullTableMeta {
  publicationName: string;
  subjectName: string;
  locations: LocationFilter[];
  timePeriodRange: TimePeriodFilter[];
  filters: Dictionary<{
    name: string;
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
  geoJsonAvailable: boolean;
}

export interface FullTable {
  title?: string;
  subjectMeta: FullTableMeta;
  results: TableDataResult[];
}
