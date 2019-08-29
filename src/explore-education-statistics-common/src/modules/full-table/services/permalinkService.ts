import { dataApi } from '@common/services/api';
import {
  TableDataQuery,
  IndicatorOption,
  TimePeriodOption,
  GroupedFilterOptions,
} from '@common/modules/full-table/services/tableBuilderService';
import { Dictionary } from '@common/types';
import { FullTable } from '../types/fullTable';
import { TableHeadersConfig } from '../utils/tableHeaders';

export interface SortableOption {
  label: string;
  value: string;
}

export interface UnmappedTableHeadersConfig {
  columnGroups: SortableOption[][];
  columns: SortableOption[];
  rowGroups: SortableOption[][];
  rows: SortableOption[];
}

interface UnmappedFullTableSubjectMeta {
  publicationName: string;
  subjectId: string;
  subjectName: string;
  locations: { label: string; value: string; level: string }[];
  timePeriodRange: TimePeriodOption[];
  filters: Dictionary<{
    legend: string;
    hint?: string;
    options: GroupedFilterOptions;
    totalValue?: string;
  }>;
  indicators: IndicatorOption[];
  footnotes: {
    id: number;
    label: string;
  }[];
}

export interface UnmappedFullTable {
  results: FullTable['results'];
  subjectMeta: UnmappedFullTableSubjectMeta;
}

export interface UnmappedPermalink {
  id: string;
  title: string;
  created: string;
  fullTable: UnmappedFullTable;
  configuration: {
    tableHeadersConfig: UnmappedTableHeadersConfig;
  };
}

export interface Permalink {
  id: string;
  title: string;
  created: string;
  fullTable: FullTable;
  configuration: {
    tableHeadersConfig: TableHeadersConfig;
  };
}

interface PermalinkCreate extends TableDataQuery {
  configuration: {
    tableHeadersConfig: UnmappedTableHeadersConfig;
  };
}

export default {
  createTablePermalink(query: PermalinkCreate): Promise<UnmappedPermalink> {
    return dataApi.post('/permalink', query);
  },
  getPermalink(publicationSlug: string): Promise<UnmappedPermalink> {
    return dataApi.get(`Permalink/${publicationSlug}`);
  },
};
