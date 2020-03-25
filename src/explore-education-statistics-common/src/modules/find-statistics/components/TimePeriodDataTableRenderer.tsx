import { mapDataBlockResponseToFullTable } from '@common/modules/find-statistics/components/util/tableUtil';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import getDefaultTableHeaderConfig, {
  TableHeadersConfig,
} from '@common/modules/table-tool/utils/tableHeaders';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { DataBlockResponse } from '@common/services/dataBlockService';
import React from 'react';

export interface Props {
  heading?: string;
  response: DataBlockResponse;
  tableHeaders?: TableHeadersConfig;
}

const TimePeriodDataTableRenderer = ({
  response,
  tableHeaders,
  heading,
}: Props) => {
  const table = mapDataBlockResponseToFullTable(response);
  return (
    <TimePeriodDataTable
      fullTable={table}
      captionTitle={heading}
      tableHeadersConfig={
        tableHeaders
          ? mapTableHeadersConfig(tableHeaders, table.subjectMeta)
          : getDefaultTableHeaderConfig(table.subjectMeta)
      }
    />
  );
};

export default TimePeriodDataTableRenderer;
