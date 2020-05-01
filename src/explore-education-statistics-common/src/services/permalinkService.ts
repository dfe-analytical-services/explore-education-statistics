import { dataApi } from '@common/services/api';
import {
  TableDataQuery,
  TableDataResponse,
} from '@common/services/tableBuilderService';

export interface UnmappedPermalink {
  id: string;
  title: string;
  created: string;
  fullTable: TableDataResponse;
  query: UnmappedPermalinkQuery;
}

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

interface UnmappedPermalinkQuery extends TableDataQuery {
  configuration: {
    tableHeaders: UnmappedTableHeadersConfig;
  };
}

export default {
  createPermalink(query: UnmappedPermalinkQuery): Promise<UnmappedPermalink> {
    return dataApi.post('/permalink', {
      ...query,
      configuration: {
        ...query.configuration,
        tableHeaders: {},
      },
    });
  },
  getPermalink(publicationSlug: string): Promise<UnmappedPermalink> {
    return dataApi.get(`/permalink/${publicationSlug}`);
  },
};
