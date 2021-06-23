import Link from '@admin/components/Link';
import { preReleaseContentRoute } from '@admin/routes/preReleaseRoutes';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import DownloadTable from '@common/modules/table-tool/components/DownloadTable';
import { BasicPublicationDetails } from '@admin/services/publicationService';
import React, { memo, useEffect, useRef, useState } from 'react';
import { generatePath } from 'react-router-dom';

interface TableToolFinalStepProps {
  publication?: BasicPublicationDetails;
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
        <>
          <DownloadTable
            fullTable={table}
            fileName={`data-${publication.slug}`}
            tableRef={dataTableRef}
          />

          <Accordion id="TableToolInfo">
            <AccordionSection heading="Related information">
              <ul className="govuk-list">
                <li>
                  <>
                    Publication:{' '}
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
                  </>
                </li>
                {/* TODO: EES-209 Add methodology page link for pre-release users */}
              </ul>
            </AccordionSection>
            {publication?.contact && (
              <AccordionSection heading="Contact us">
                <p>
                  If you have a question about the data or methods used to
                  create this table contact the named statistician:
                </p>
                <h4 className="govuk-heading-s govuk-!-font-weight-bold">
                  {publication?.contact.teamName}
                </h4>
                <p>Named statistician: {publication?.contact.contactName}</p>
                <p>
                  Email:{' '}
                  <a href={`mailto:${publication?.contact.teamEmail}`}>
                    {publication?.contact.teamEmail}
                  </a>
                </p>
                <p>Telephone: {publication?.contact.contactTelNo}</p>
              </AccordionSection>
            )}
          </Accordion>
        </>
      )}
    </div>
  );
};

export default memo(PreReleaseTableToolFinalStep);
