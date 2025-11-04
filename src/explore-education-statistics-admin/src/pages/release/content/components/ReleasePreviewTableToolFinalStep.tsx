import { Publication } from '@admin/services/publicationService';
import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import DownloadTable from '@common/modules/table-tool/components/DownloadTable';
import tableBuilderService, {
  ReleaseTableDataQuery,
} from '@common/services/tableBuilderService';
import React, { useRef } from 'react';

interface ReleasePreviewTableToolFinalStepProps {
  publication: Publication;
  query: ReleaseTableDataQuery;
  table: FullTable;
  tableHeaders: TableHeadersConfig;
  onReorderTableHeaders: (reorderedTableHeaders: TableHeadersConfig) => void;
}
const ReleasePreviewTableToolFinalStep = ({
  publication,
  query,
  table,
  tableHeaders,
  onReorderTableHeaders,
}: ReleasePreviewTableToolFinalStepProps) => {
  const dataTableRef = useRef<HTMLElement>(null);

  return (
    <div className="govuk-!-margin-bottom-4">
      {table && tableHeaders && (
        <TimePeriodDataTable
          ref={dataTableRef}
          footnotesClassName="govuk-!-width-two-thirds"
          fullTable={table}
          query={query}
          tableHeadersConfig={tableHeaders}
          tableHeadersForm={
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
          }
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

export default ReleasePreviewTableToolFinalStep;
