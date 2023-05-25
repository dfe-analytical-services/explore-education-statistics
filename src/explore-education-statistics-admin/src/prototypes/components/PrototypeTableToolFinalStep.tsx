import Button from '@admin/prototypes/components/PrototypeButton';
import Tag from '@common/components/Tag';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import useToggle from '@common/hooks/useToggle';
import DownloadTable from '@common/modules/table-tool/components/DownloadTable';
import TableHeadersForm from '@admin/prototypes/components/PrototypeTableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import TableToolInfo from '@common/modules/table-tool/components/TableToolInfo';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import publicationService from '@common/services/publicationService';
import Link from '@admin/components/Link';
import tableBuilderService, {
  ReleaseTableDataQuery,
  SelectedPublication,
} from '@common/services/tableBuilderService';
import React, { memo, ReactNode, useEffect, useRef, useState } from 'react';

interface TableToolFinalStepProps {
  query: ReleaseTableDataQuery;
  table: FullTable;
  tableHeaders: TableHeadersConfig;
  selectedPublication: SelectedPublication;
}

const TableToolFinalStep = ({
  query,
  table,
  tableHeaders,
  selectedPublication,
}: TableToolFinalStepProps) => {
  const dataTableRef = useRef<HTMLElement>(null);
  const hasTableError = false;
  const [currentTableHeaders, setCurrentTableHeaders] = useState<
    TableHeadersConfig
  >();
  const [showTableHeadersForm, toggleShowTableHeadersForm] = useToggle(false);

  const tableHeadersFormId = 'tableHeaderForm';

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
      {table && currentTableHeaders && (
        <>
          <div className="govuk-!-margin-bottom-3 dfe-flex dfe-align-items-start dfe-justify-content--space-between">
            {selectedPublication.selectedRelease.latestData && (
              <Tag strong>This is the latest data</Tag>
            )}

            {!selectedPublication.selectedRelease.latestData && (
              <div>
                <div className="govuk-!-margin-bottom-3">
                  <Tag strong colour="orange">
                    This data is not from the latest release
                  </Tag>
                </div>

                <Link
                  className="dfe-print-hidden"
                  unvisited
                  to={`/find-statistics/${selectedPublication.slug}`}
                  // testId="View latest data link"
                >
                  View latest data:{' '}
                  <span className="govuk-!-font-weight-bold">
                    {selectedPublication.latestRelease.title}
                  </span>
                </Link>
              </div>
            )}
            {!showTableHeadersForm && (
              <Button
                className="govuk-!-margin-bottom-0"
                ariaControls={tableHeadersFormId}
                ariaExpanded={showTableHeadersForm}
                onClick={toggleShowTableHeadersForm}
              >
                Move and reorder table headers
              </Button>
            )}
          </div>
          <TableHeadersForm
            id={tableHeadersFormId}
            initialValues={currentTableHeaders}
            showTableHeadersForm={showTableHeadersForm}
            onSubmit={tableHeaderConfig => {
              toggleShowTableHeadersForm.off();
              setCurrentTableHeaders(tableHeaderConfig);
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

          <TimePeriodDataTable
            ref={dataTableRef}
            fullTable={table}
            tableHeadersConfig={currentTableHeaders}
          />
        </>
      )}
      {!hasTableError && (
        <>
          <div className="govuk-!-margin-bottom-7">
            {/* <TableToolShare
              tableHeaders={currentTableHeaders}
              query={query}
              selectedPublication={selectedPublication}
            /> */}
          </div>

          <DownloadTable
            fullTable={table}
            fileName={`data-${selectedPublication.slug}`}
            onCsvDownload={() => tableBuilderService.getTableCsv(query)}
            tableRef={dataTableRef}
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
