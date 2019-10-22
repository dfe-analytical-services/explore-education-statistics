/* eslint-disable @typescript-eslint/no-unused-vars */
import React from 'react';
import {
  DataBlockData,
  DataBlockMetadata,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { mapDataBlockResponseToFullTable } from '@common/modules/find-statistics/components/util/tableUtil';
import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import getDefaultTableHeaderConfig from '@common/modules/full-table/utils/tableHeaders';

export interface Props {
  heading?: string;
  response: DataBlockResponse;
  tableHeaders?: TableHeadersFormValues;
}

// eslint-disable-next-line @typescript-eslint/no-unused-vars
const TableRenderer = ({ response, tableHeaders }: Props) => {
  const table = mapDataBlockResponseToFullTable(response);

  const usedTableHeaders =
    tableHeaders || getDefaultTableHeaderConfig(table.subjectMeta);

  return (
    <TimePeriodDataTable
      fullTable={table}
      tableHeadersConfig={usedTableHeaders}
    />
  );
};

export default TableRenderer;
