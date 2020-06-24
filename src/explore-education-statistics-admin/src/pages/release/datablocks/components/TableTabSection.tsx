import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import React, { useRef } from 'react';

interface Props {
  table: FullTable;
  tableHeaders: TableHeadersConfig;
  onSave: (tableHeaders: TableHeadersConfig) => void;
}

const TableTabSection = ({ table, tableHeaders, onSave }: Props) => {
  const dataTableRef = useRef<HTMLElement>(null);

  return (
    <>
      <TableHeadersForm
        initialValues={tableHeaders}
        id="dataBlockContentTabs-tableHeadersForm"
        onSubmit={async nextTableHeaders => {
          await onSave(nextTableHeaders);

          if (dataTableRef.current) {
            dataTableRef.current.scrollIntoView({
              behavior: 'smooth',
              block: 'start',
            });
          }
        }}
      />

      <TimePeriodDataTable
        fullTable={table}
        tableHeadersConfig={tableHeaders}
        ref={dataTableRef}
      />
    </>
  );
};

export default TableTabSection;
