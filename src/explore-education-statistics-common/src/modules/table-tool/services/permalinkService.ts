import {
  TableDataQuery,
  UnmappedFullTable,
} from '@common/modules/table-tool/services/tableBuilderService';
import { UnmappedTableHeadersConfig } from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import { dataApi } from '@common/services/api';

export interface UnmappedPermalink {
  id: string;
  title: string;
  created: string;
  fullTable: UnmappedFullTable;
  query: PermalinkCreateQuery;
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
