import Link from '@admin/components/Link';
import { preReleaseContentRoute } from '@admin/routes/preReleaseRoutes';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import { BasicPublicationDetails } from '@admin/services/publicationService';
import Button from '@common/components/Button';
import useToggle from '@common/hooks/useToggle';
import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import DownloadTable from '@common/modules/table-tool/components/DownloadTable';
import TableToolInfo from '@common/modules/table-tool/components/TableToolInfo';
import { ReleaseTableDataQuery } from '@common/services/tableBuilderService';
import React, { memo, useRef } from 'react';
import { generatePath } from 'react-router-dom';

interface TableToolFinalStepProps {
  publication?: BasicPublicationDetails;
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
  const [showTableHeadersForm, toggleShowTableHeadersForm] = useToggle(false);

  const tableHeadersFormId = 'tableHeadersForm';

  return (
    <div className="govuk-!-margin-bottom-4">
      {!showTableHeadersForm ? (
        <div className="govuk-!-margin-bottom-3 dfe-flex dfe-justify-content--flex-end ">
          <Button
            className="govuk-!-margin-bottom-0"
            ariaControls={tableHeadersFormId}
            ariaExpanded={showTableHeadersForm}
            onClick={toggleShowTableHeadersForm}
          >
            Move and reorder table headers
          </Button>
        </div>
      ) : (
        <TableHeadersForm
          id={tableHeadersFormId}
          initialValues={tableHeaders}
          onSubmit={nextTableHeaders => {
            toggleShowTableHeadersForm.off();
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
      )}
      {table && tableHeaders && (
        <TimePeriodDataTable
          ref={dataTableRef}
          query={query}
          fullTable={table}
          tableHeadersConfig={tableHeaders}
        />
      )}

      {publication && table && (
        <>
          <DownloadTable
            fullTable={table}
            fileName={`data-${publication.slug}`}
            tableRef={dataTableRef}
          />

          <TableToolInfo
            contactDetails={publication.contact}
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
        </>
      )}
    </div>
  );
};

export default memo(PreReleaseTableToolFinalStep);
