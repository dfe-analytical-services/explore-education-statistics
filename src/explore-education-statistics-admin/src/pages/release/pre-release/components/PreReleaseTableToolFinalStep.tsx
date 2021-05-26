import Link from '@admin/components/Link';
import { preReleaseContentRoute } from '@admin/routes/preReleaseRoutes';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import DownloadCsvButton from '@common/modules/table-tool/components/DownloadCsvButton';
import DownloadExcelButton from '@common/modules/table-tool/components/DownloadExcelButton';
import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import React, { memo, useEffect, useRef, useState } from 'react';
import { generatePath } from 'react-router-dom';

interface TableToolFinalStepProps {
  publication?: {
    id: string;
    slug: string;
  };
  releaseId: string;
  table: FullTable;
  tableHeaders: TableHeadersConfig;
}

const PreReleaseTableToolFinalStep = ({
  publication,
  releaseId,
  table,
  tableHeaders,
}: TableToolFinalStepProps) => {
  const dataTableRef = useRef<HTMLElement>(null);
  const [currentTableHeaders, setCurrentTableHeaders] = useState<
    TableHeadersConfig
  >();

  useEffect(() => {
    setCurrentTableHeaders(tableHeaders);
  }, [tableHeaders]);

  return (
    <div className="govuk-!-margin-bottom-4">
      <TableHeadersForm
        initialValues={currentTableHeaders}
        onSubmit={tableHeaderConfig => {
          setCurrentTableHeaders(tableHeaderConfig);

          if (dataTableRef.current) {
            dataTableRef.current.scrollIntoView({
              behavior: 'smooth',
              block: 'start',
            });
          }
        }}
      />
      {table && currentTableHeaders && (
        <TimePeriodDataTable
          ref={dataTableRef}
          fullTable={table}
          tableHeadersConfig={currentTableHeaders}
        />
      )}

      <h3>Additional options</h3>

      {publication && table && (
        <ul className="govuk-list">
          <li>
            <Link
              to={generatePath<ReleaseRouteParams>(
                preReleaseContentRoute.path,
                {
                  publicationId: publication.id,
                  releaseId,
                },
              )}
            >
              View the release for this data
            </Link>
          </li>
          <li>
            <DownloadCsvButton
              fileName={`data-${publication.slug}`}
              fullTable={table}
            />
          </li>
          <li>
            <DownloadExcelButton
              fileName={`data-${publication.slug}`}
              tableRef={dataTableRef}
              subjectMeta={table.subjectMeta}
            />
          </li>
          {/* TODO: EES-209 Add methodology page link for pre-release users */}
        </ul>
      )}

      <p>
        If you have a question about the data or methods used to create this
        table contact the named statistician via the relevant release page.
      </p>
    </div>
  );
};

export default memo(PreReleaseTableToolFinalStep);
