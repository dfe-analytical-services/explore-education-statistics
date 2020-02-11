import { TableDataQuery } from '@common/modules/table-tool/services/tableBuilderService';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/utils/tableHeaders';

interface PermalinkQuery extends TableDataQuery {
  configuration: {
    tableHeadersConfig: TableHeadersConfig;
  };
}

export interface Permalink {
  id: string;
  title: string;
  created: string;
  fullTable: FullTable;
  query: PermalinkQuery;
}
