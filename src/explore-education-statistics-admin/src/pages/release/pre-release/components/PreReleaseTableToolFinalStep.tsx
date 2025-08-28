import { Publication } from '@admin/services/publicationService';
import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import DownloadTable from '@common/modules/table-tool/components/DownloadTable';
import tableBuilderService, {
  ReleaseTableDataQuery,
} from '@common/services/tableBuilderService';
import React, { memo, useRef } from 'react';

interface TableToolFinalStepProps {
  publication?: Publication;
  query: ReleaseTableDataQuery;
  table: FullTable;
  tableHeaders: TableHeadersConfig;
  onReorderTableHeaders: (reorderedTableHeaders: TableHeadersConfig) => void;
}

const PreReleaseTableToolFinalStep = ({
  publication,
  query,
  table,
  tableHeaders,
  onReorderTableHeaders,
}: TableToolFinalStepProps) => {
  const dataTableRef = useRef<HTMLElement>(null);

  return (
    <div className="govuk-!-margin-bottom-4">
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
      {table && tableHeaders && (
        <TimePeriodDataTable
          ref={dataTableRef}
          query={query}
          footnotesClassName="govuk-!-width-two-thirds"
          fullTable={table}
          tableHeadersConfig={tableHeaders}
        />
      )}

      {publication && table && (
        <DownloadTable
          fullTable={table}
          fileName={`data-${publication.slug}`}
          onCsvDownload={() => tableBuilderService.getTableCsv(query)}
          tableRef={dataTableRef}
        />
      )}
    </div>
  );
};

export default memo(PreReleaseTableToolFinalStep);
