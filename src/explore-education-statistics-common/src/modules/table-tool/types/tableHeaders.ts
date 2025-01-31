import { Filter } from '@common/modules/table-tool/types/filters';

export interface TableHeadersConfig {
  columns: Filter[];
  columnGroups: Filter[][];
  rows: Filter[];
  rowGroups: Filter[][];
}
