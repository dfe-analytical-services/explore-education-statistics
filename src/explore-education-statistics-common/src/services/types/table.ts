import { UnmappedTableHeadersConfig } from '@common/services/permalinkService';
import { TableDataResponse } from '@common/services/tableBuilderService';

export interface ConfiguredTable {
  id: string;
  fullTable: TableDataResponse;
  configuration: {
    tableHeaders: UnmappedTableHeadersConfig;
  };
}
