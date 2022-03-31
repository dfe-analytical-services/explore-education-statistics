import Link from '@admin/components/Link';
import { preReleaseContentRoute } from '@admin/routes/preReleaseRoutes';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import { BasicPublicationDetails } from '@admin/services/publicationService';
import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import DownloadTable from '@common/modules/table-tool/components/DownloadTable';
import TableToolInfo from '@common/modules/table-tool/components/TableToolInfo';
import { ReleaseTableDataQuery } from '@common/services/tableBuilderService';
import React, { memo, useEffect, useRef, useState } from 'react';
import { generatePath } from 'react-router-dom';

interface TableToolFinalStepProps {
  publication?: BasicPublicationDetails;
  query: ReleaseTableDataQuery;
  releaseId: string;
  table: FullTable;
  tableHeaders: TableHeadersConfig;
}

const PreReleaseTableToolFinalStep = ({
  publication,
  query,
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
          query={query}
          fullTable={table}
          tableHeadersConfig={currentTableHeaders}
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
