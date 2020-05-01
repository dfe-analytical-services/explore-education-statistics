import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import { TableDataQuery } from '@common/services/tableBuilderService';
import { FullTable } from '@common/modules/table-tool/types/fullTable';

interface PermalinkQuery extends TableDataQuery {
  configuration: {
    tableHeaders: TableHeadersConfig;
  };
}

export interface Permalink {
  id: string;
  title: string;
  created: string;
  fullTable: FullTable;
  query: PermalinkQuery;
}
