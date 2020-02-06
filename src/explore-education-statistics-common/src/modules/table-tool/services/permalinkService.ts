import {
  FilterOption,
  GroupedFilterOptions,
  IndicatorOption,
  TableDataQuery,
  TimePeriodOption,
} from '@common/modules/table-tool/services/tableBuilderService';
import { dataApi } from '@common/services/api';
import { Dictionary } from '@common/types';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/utils/tableHeaders';

export interface UnmappedTableHeadersConfig {
  columnGroups: FilterOption[][];
  columns: FilterOption[];
  rowGroups: FilterOption[][];
  rows: FilterOption[];
}

interface UnmappedFullTableSubjectMeta {
  publicationName: string;
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
    id: string;
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
  query: PermalinkCreateQuery;
}

export interface Permalink {
  id: string;
  title: string;
  created: string;
  fullTable: FullTable;
  query: PermalinkQuery;
}

interface PermalinkCreateQuery extends TableDataQuery {
  configuration: {
    tableHeadersConfig: UnmappedTableHeadersConfig;
  };
}

interface PermalinkQuery extends TableDataQuery {
  configuration: {
    tableHeadersConfig: TableHeadersConfig;
  };
}

export default {
  createTablePermalink(
    query: PermalinkCreateQuery,
  ): Promise<UnmappedPermalink> {
    return dataApi.post('/permalink', query);
  },
  getPermalink(publicationSlug: string): Promise<UnmappedPermalink> {
    return dataApi.get(`Permalink/${publicationSlug}`);
  },
};
