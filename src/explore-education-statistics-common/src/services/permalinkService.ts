import { dataApi } from '@common/services/api';
import { TableDataQuery } from '@common/services/tableBuilderService';
import { ConfiguredTable } from '@common/services/types/table';

export type Permalink = ConfiguredTable;

export type TableHeader =
  | {
      type: 'TimePeriod' | 'Indicator' | 'Filter';
      value: string;
    }
  | {
      type: 'Location';
      value: string;
      level: string;
    };

export interface UnmappedTableHeadersConfig {
  columnGroups: TableHeader[][];
  columns: TableHeader[];
  rowGroups: TableHeader[][];
  rows: TableHeader[];
}

interface CreatePermalink {
  query: TableDataQuery;
  configuration: {
    tableHeaders: UnmappedTableHeadersConfig;
  };
}

export default {
  createPermalink(
    query: CreatePermalink,
    releaseId?: string,
  ): Promise<Permalink> {
    if (releaseId) {
      return dataApi.post(`/release/${releaseId}/permalink`, query);
    }
    return dataApi.post(`/permalink`, query);
  },
  getPermalink(publicationSlug: string): Promise<Permalink> {
    return dataApi.get(`/permalink/${publicationSlug}`);
  },
};
