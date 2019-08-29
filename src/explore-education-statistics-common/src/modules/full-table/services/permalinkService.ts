import { dataApi } from '@common/services/api';
import { Dictionary } from '@common/types';
import {
  TableData,
  GroupedFilterOptions,
  TableDataQuery,
  FilterOption,
} from '@common/modules/full-table/services/tableBuilderService';
import {
  Indicator,
  LocationFilter,
  CategoryFilter,
  TimePeriodFilter,
} from '@common/modules/full-table/types/filters';
import { transformTableMetaFiltersToCategoryFilters } from '@common/modules/full-table/utils/tableHeaders';

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

interface PermalinkCreate extends TableDataQuery {
  configuration: {
    tableHeadersConfig: UnmappedTableHeadersConfig;
  };
}

export interface FullTable extends TableData {
  title?: string;
}

export interface Permalink {
  id: string;
  title: string;
  created: string;
  fullTable: FullTable;
  configuration: {
    tableHeadersConfig: UnmappedTableHeadersConfig;
  };
}

interface UnmappedTableHeadersConfig {
  columnGroups: SortableOption[][];
  columns: SortableOption[];
  rowGroups: SortableOption[][];
  rows: SortableOption[];
}

interface SortableOption {
  label: string;
  value: string;
}

export default {
  createTablePermalink(query: PermalinkCreate): Promise<Permalink> {
    return dataApi.post('/permalink', query);
  },
  getPermalink(publicationSlug: string): Promise<Permalink> {
    return dataApi.get(`Permalink/${publicationSlug}`);
  },
};
