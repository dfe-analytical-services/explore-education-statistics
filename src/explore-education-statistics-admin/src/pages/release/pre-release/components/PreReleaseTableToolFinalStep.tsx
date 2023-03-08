import Link from '@admin/components/Link';
import { preReleaseContentRoute } from '@admin/routes/preReleaseRoutes';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import publicationService, {
  Contact,
  Publication,
} from '@admin/services/publicationService';
import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import DownloadTable from '@common/modules/table-tool/components/DownloadTable';
import TableToolInfo from '@common/modules/table-tool/components/TableToolInfo';
import tableBuilderService, {
  ReleaseTableDataQuery,
} from '@common/services/tableBuilderService';
import React, { memo, useRef } from 'react';
import { generatePath } from 'react-router-dom';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import LoadingSpinner from '@common/components/LoadingSpinner';

interface TableToolFinalStepProps {
  publication?: Publication;
  query: ReleaseTableDataQuery;
  releaseId: string;
  table: FullTable;
  tableHeaders: TableHeadersConfig;
  onReorderTableHeaders: (reorderedTableHeaders: TableHeadersConfig) => void;
}

const PreReleaseTableToolFinalStep = ({
  publication,
  query,
  releaseId,
  table,
  tableHeaders,
  onReorderTableHeaders,
}: TableToolFinalStepProps) => {
  const dataTableRef = useRef<HTMLElement>(null);

  const { value: contact, isLoading } = useAsyncHandledRetry<
    Contact | undefined
  >(async () => {
    if (!publication) {
      return undefined;
    }
    return publicationService.getContact(publication.id);
  }, [publication]);

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
        <>
          <DownloadTable
            fullTable={table}
            fileName={`data-${publication.slug}`}
            onCsvDownload={() => tableBuilderService.getTableCsv(query)}
            tableRef={dataTableRef}
          />

          <LoadingSpinner loading={isLoading}>
            <TableToolInfo
              contactDetails={contact}
              releaseLink={
                <Link
                  to={generatePath<ReleaseRouteParams>(
                    preReleaseContentRoute.path,
                    {
                      publicationId: publication.id,
                      releaseId,
                    },
                  )}
                >
                  {publication.title}
                </Link>
              }
            />
          </LoadingSpinner>
        </>
      )}
    </div>
  );
};

export default memo(PreReleaseTableToolFinalStep);
