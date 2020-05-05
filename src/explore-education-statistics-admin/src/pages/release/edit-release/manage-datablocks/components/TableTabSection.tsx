import { SavedDataBlock } from '@admin/pages/release/edit-release/manage-datablocks/components/DataBlockSourceWizard';
import { ReleaseDataBlock } from '@admin/services/release/edit-release/datablocks/service';
import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import mapUnmappedTableHeaders from '@common/modules/table-tool/utils/mapUnmappedTableHeaders';
import React, { useRef } from 'react';

interface Props {
  dataBlock: ReleaseDataBlock;
  table: FullTable;
  tableHeaders: TableHeadersConfig;
  onDataBlockSave: (dataBlock: SavedDataBlock) => void;
}

const TableTabSection = ({
  dataBlock,
  table,
  tableHeaders,
  onDataBlockSave,
}: Props) => {
  const dataTableRef = useRef<HTMLElement>(null);

  return (
    <>
      <TableHeadersForm
        initialValues={tableHeaders}
        id="dataBlockContentTabs-tableHeadersForm"
        onSubmit={async nextTableHeaders => {
          await onDataBlockSave({
            ...dataBlock,
            tables: [
              {
                tableHeaders: mapUnmappedTableHeaders(nextTableHeaders),
                indicators: [],
              },
            ],
          });

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
