import { ReleaseDataBlock } from '@admin/services/dataBlockService';
import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import React, { useRef } from 'react';

interface Props {
  dataBlock: ReleaseDataBlock;
  table: FullTable;
  tableHeaders: TableHeadersConfig;
  onReorderTableHeaders?: (tableHeaders: TableHeadersConfig) => void;
}

const TableTabSection = ({
  dataBlock,
  table,
  tableHeaders,
  onReorderTableHeaders,
}: Props) => {
  const dataTableRef = useRef<HTMLElement>(null);

  return (
    <>
      {onReorderTableHeaders && (
        <>
          <TableHeadersForm
            initialValues={tableHeaders}
            onSubmit={nextTableHeaders => {
              onReorderTableHeaders(nextTableHeaders);
              if (dataTableRef.current) {
                // add a short delay so the reordering form is closed before it scrolls.
                setTimeout(() => {
                  dataTableRef?.current?.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start',
                  });
                }, 200);
              }
            }}
          />
        </>
      )}

      <TimePeriodDataTable
        footnotesClassName="govuk-!-width-two-thirds"
        fullTable={table}
        tableHeadersConfig={tableHeaders}
        captionTitle={dataBlock.heading}
        source={dataBlock.source}
        ref={dataTableRef}
      />
    </>
  );
};

export default TableTabSection;
