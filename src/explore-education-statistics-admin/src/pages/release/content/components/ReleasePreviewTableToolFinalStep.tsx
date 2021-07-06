import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import DownloadTable from '@common/modules/table-tool/components/DownloadTable';
import TableToolInfo from '@common/modules/table-tool/components/TableToolInfo';
import Link from '@admin/components/Link';
import { BasicPublicationDetails } from '@admin/services/publicationService';
import React, { useEffect, useRef, useState } from 'react';

interface ReleasePreviewTableToolFinalStepProps {
  publication?: BasicPublicationDetails;
  table: FullTable;
  tableHeaders: TableHeadersConfig;
}
const ReleasePreviewTableToolFinalStep = ({
  publication,
  table,
  tableHeaders,
}: ReleasePreviewTableToolFinalStepProps) => {
  const dataTableRef = useRef<HTMLElement>(null);
  const [currentTableHeaders, setCurrentTableHeaders] = useState<
    TableHeadersConfig
  >();

  useEffect(() => {
    setCurrentTableHeaders(tableHeaders);
  }, [tableHeaders]);

  const getMethodologyLinks = () => {
    if (publication?.methodologies && publication?.methodologies.length) {
      return publication?.methodologies.map(methodology => methodology.title);
    }
    if (publication?.externalMethodology) {
      return [
        <Link
          key={publication.externalMethodology.url}
          to={publication.externalMethodology.url}
        >
          {publication.externalMethodology.title}
        </Link>,
      ];
    }
    return [];
  };

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
      {publication && table && (
        <>
          <DownloadTable
            fullTable={table}
            fileName={`data-${publication.slug}`}
            tableRef={dataTableRef}
          />

          <TableToolInfo
            contactDetails={publication.contact}
            methodologyLinks={getMethodologyLinks()}
            releaseLink={<span>{publication.title}</span>}
          />
        </>
      )}
    </div>
  );
};

export default ReleasePreviewTableToolFinalStep;
