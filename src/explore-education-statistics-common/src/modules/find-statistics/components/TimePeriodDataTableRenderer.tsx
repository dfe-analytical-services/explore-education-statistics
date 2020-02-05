import { mapDataBlockResponseToFullTable } from '@common/modules/find-statistics/components/util/tableUtil';
import getDefaultTableHeaderConfig, {
  TableHeadersConfig,
} from '@common/modules/full-table/utils/tableHeaders';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { DataBlockResponse } from '@common/services/dataBlockService';
import React from 'react';

export interface Props {
  heading?: string;
  response: DataBlockResponse;
  tableHeaders?: TableHeadersConfig;
}

const TimePeriodDataTableRenderer = ({ response, tableHeaders }: Props) => {
  const table = mapDataBlockResponseToFullTable(response);

  return (
    <TimePeriodDataTable
      fullTable={table}
      tableHeadersConfig={
        tableHeaders ?? getDefaultTableHeaderConfig(table.subjectMeta)
      }
    />
  );
};

export default TimePeriodDataTableRenderer;
