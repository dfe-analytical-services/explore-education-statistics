import { UnmappedTableHeadersConfig } from '@common/services/permalinkService';
import {
  TableDataQuery,
  TableDataResponse,
} from '@common/services/tableBuilderService';

export interface ConfiguredTable {
  id: string;
  created: string;
  fullTable: TableDataResponse;
  configuration: {
    tableHeaders: UnmappedTableHeadersConfig;
  };
  query: TableDataQuery;
}
