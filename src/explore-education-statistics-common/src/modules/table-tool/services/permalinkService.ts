import {
  TableDataQuery,
  UnmappedFullTable,
} from '@common/modules/table-tool/services/tableBuilderService';
import { dataApi } from '@common/services/api';

export interface UnmappedPermalink {
  id: string;
  title: string;
  created: string;
  fullTable: UnmappedFullTable;
  query: PermalinkCreateQuery;
}

interface HeaderOption {
  value: string;
  label: string;
}

// TODO: We should re-work this to store type information
//  in the backend so that its easier to work with
export interface UnmappedTableHeadersConfig {
  columnGroups: HeaderOption[][];
  columns: HeaderOption[];
  rowGroups: HeaderOption[][];
  rows: HeaderOption[];
}

interface PermalinkCreateQuery extends TableDataQuery {
  configuration: {
    tableHeadersConfig: UnmappedTableHeadersConfig;
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
