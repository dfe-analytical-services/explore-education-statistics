import Tag from '@common/components/Tag';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import useToggle from '@common/hooks/useToggle';
import DownloadTable, {
  FileFormat,
} from '@common/modules/table-tool/components/DownloadTable';
import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import TableToolShare from '@frontend/modules/table-tool/components/TableToolShare';
import TableToolInfo from '@common/modules/table-tool/components/TableToolInfo';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import publicationService from '@common/services/publicationService';
import Link from '@frontend/components/Link';
import {
  SelectedPublication,
  TableDataQuery,
} from '@common/services/tableBuilderService';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import React, { memo, ReactNode, useEffect, useRef, useState } from 'react';

interface TableToolFinalStepProps {
  query: TableDataQuery;
  table: FullTable;
  tableHeaders: TableHeadersConfig;
  selectedPublication: SelectedPublication;
}

const TableToolFinalStep = ({
  table,
  tableHeaders,
  query,
  selectedPublication,
}: TableToolFinalStepProps) => {
  const dataTableRef = useRef<HTMLElement>(null);
  const [hasTableError, toggleHasTableError] = useToggle(false);
  const [currentTableHeaders, setCurrentTableHeaders] = useState<
    TableHeadersConfig
  >();

  useEffect(() => {
    setCurrentTableHeaders(tableHeaders);
  }, [tableHeaders]);

  const { value: fullPublication } = useAsyncRetry(
    async () =>
      publicationService.getLatestPublicationRelease(selectedPublication.slug),
    [selectedPublication],
  );
  const publication = fullPublication?.publication;

  const getMethodologyLinks = () => {
    const links: ReactNode[] =
      publication?.methodologies?.map(methodology => (
        <Link key={methodology.id} to={`/methodology/${methodology.slug}`}>
          {methodology.title}
        </Link>
      )) ?? [];

    if (publication?.externalMethodology) {
      links.push(
        <Link
          key={publication.externalMethodology.url}
          to={publication.externalMethodology.url}
        >
          {publication.externalMethodology.title}
        </Link>,
      );
    }
    return links;
  };

  return (
    <div
      className="govuk-!-margin-bottom-4"
      data-testid="Table tool final step container"
    >
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
        <>
          <div className="govuk-!-margin-bottom-3">
            {selectedPublication.selectedRelease.latestData && (
              <Tag strong>This is the latest data</Tag>
            )}

            {!selectedPublication.selectedRelease.latestData && (
              <>
                <div className="govuk-!-margin-bottom-3">
                  <Tag strong colour="orange">
                    This data is not from the latest release
                  </Tag>
                </div>

                <Link
                  className="dfe-print-hidden"
                  unvisited
                  to={`/find-statistics/${selectedPublication.slug}`}
                  testId="View latest data link"
                >
                  View latest data:{' '}
                  <span className="govuk-!-font-weight-bold">
                    {selectedPublication.latestRelease.title}
                  </span>
                </Link>
              </>
            )}
          </div>

          <TimePeriodDataTable
            ref={dataTableRef}
            fullTable={table}
            tableHeadersConfig={currentTableHeaders}
            onError={message => {
              toggleHasTableError.on();
              logEvent({
                category: 'Table Tool',
                action: 'Table rendering error',
                label: message,
              });
            }}
          />
        </>
      )}
      {!hasTableError && (
        <>
          <div className="govuk-!-margin-bottom-7">
            <TableToolShare
              tableHeaders={currentTableHeaders}
              query={query}
              selectedPublication={selectedPublication}
            />
          </div>

          <DownloadTable
            fullTable={table}
            fileName={`data-${selectedPublication.slug}`}
            tableRef={dataTableRef}
            onSubmit={(fileFormat: FileFormat) =>
              logEvent({
                category: 'Table tool',
                action:
                  fileFormat === 'csv'
                    ? 'CSV download button clicked'
                    : 'ODS download button clicked',
                label: `${table.subjectMeta.publicationName} between ${
                  table.subjectMeta.timePeriodRange[0].label
                } and ${
                  table.subjectMeta.timePeriodRange[
                    table.subjectMeta.timePeriodRange.length - 1
                  ].label
                }`,
              })
            }
          />
        </>
      )}

      <TableToolInfo
        contactDetails={publication?.contact}
        methodologyLinks={getMethodologyLinks()}
        releaseLink={
          <>
            {selectedPublication.selectedRelease.latestData ? (
              <Link to={`/find-statistics/${selectedPublication.slug}`}>
                {`${selectedPublication.title}, ${selectedPublication.selectedRelease.title}`}
              </Link>
            ) : (
              <Link
                to={`/find-statistics/${selectedPublication.slug}/${selectedPublication.selectedRelease.slug}`}
              >
                {`${selectedPublication.title}, ${selectedPublication.selectedRelease.title}`}
              </Link>
            )}
          </>
        }
      />
    </div>
  );
};

export default memo(TableToolFinalStep);
