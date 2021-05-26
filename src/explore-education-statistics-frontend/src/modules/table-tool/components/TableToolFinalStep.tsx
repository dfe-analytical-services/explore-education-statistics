import ButtonText from '@common/components/ButtonText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import UrlContainer from '@common/components/UrlContainer';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import DownloadCsvButton from '@common/modules/table-tool/components/DownloadCsvButton';
import DownloadExcelButton from '@common/modules/table-tool/components/DownloadExcelButton';
import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import mapUnmappedTableHeaders from '@common/modules/table-tool/utils/mapUnmappedTableHeaders';
import permalinkService from '@common/services/permalinkService';
import publicationService from '@common/services/publicationService';
import {
  SelectedPublication,
  TableDataQuery,
} from '@common/services/tableBuilderService';
import Link from '@frontend/components/Link';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import React, { memo, useEffect, useRef, useState } from 'react';

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
  const [permalinkId, setPermalinkId] = useState<string>('');
  const [permalinkLoading, setPermalinkLoading] = useState<boolean>(false);
  const [currentTableHeaders, setCurrentTableHeaders] = useState<
    TableHeadersConfig
  >();

  useEffect(() => {
    setCurrentTableHeaders(tableHeaders);
    setPermalinkId('');
  }, [tableHeaders]);

  const { value: pubMethodology } = useAsyncRetry(
    async () =>
      publicationService.getPublicationMethodology(selectedPublication.slug),
    [selectedPublication],
  );

  const handlePermalinkClick = async () => {
    if (!currentTableHeaders) {
      return;
    }
    setPermalinkLoading(true);

    const { id } = await permalinkService.createPermalink(
      {
        query,
        configuration: {
          tableHeaders: mapUnmappedTableHeaders(currentTableHeaders),
        },
      },
      selectedPublication.selectedRelease.id,
    );

    setPermalinkId(id);
    setPermalinkLoading(false);
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
          setPermalinkId('');
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
                  to="/find-statistics/[publication]"
                  as={`/find-statistics/${selectedPublication.slug}`}
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
          />
        </>
      )}

      <h3>Share your table</h3>
      <ul className="govuk-list">
        <li>
          {permalinkId ? (
            <>
              <p className="govuk-!-margin-bottom-2">
                Generated permanent link:
              </p>

              <p className="govuk-!-margin-top-0 govuk-!-margin-bottom-2">
                <UrlContainer
                  data-testid="permalink-generated-url"
                  url={`${window.location.origin}/data-tables/permalink/${permalinkId}`}
                />
              </p>

              <Link
                className="govuk-!-margin-top-0"
                to="/data-tables/permalink/[permalink]"
                as={`/data-tables/permalink/${permalinkId}`}
                title="View created table permalink"
                target="_blank"
                rel="noopener noreferrer"
              >
                View permanent link
              </Link>
            </>
          ) : (
            <LoadingSpinner
              alert
              inline
              loading={permalinkLoading}
              size="sm"
              text="Generating permanent link"
            >
              <ButtonText onClick={handlePermalinkClick}>
                Generate permanent link
              </ButtonText>
            </LoadingSpinner>
          )}
        </li>
      </ul>

      <h3>Additional options</h3>
      {table && (
        <ul className="govuk-list">
          <li>
            {selectedPublication.selectedRelease.latestData ? (
              <Link
                to="/find-statistics/[publication]"
                as={`/find-statistics/${selectedPublication.slug}`}
              >
                View the release for this data
              </Link>
            ) : (
              <Link
                to="/find-statistics/[publication]/[releaseSlug]"
                as={`/find-statistics/${selectedPublication.slug}/${selectedPublication.selectedRelease.slug}`}
              >
                View the release for this data
              </Link>
            )}
          </li>
          <li>
            <DownloadCsvButton
              fileName={`data-${selectedPublication.slug}`}
              fullTable={table}
              onClick={() =>
                logEvent({
                  category: 'Table tool',
                  action: 'CSV download button clicked',
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
          </li>
          <li>
            <DownloadExcelButton
              fileName={`data-${selectedPublication.slug}`}
              tableRef={dataTableRef}
              subjectMeta={table.subjectMeta}
              onClick={() =>
                logEvent({
                  category: 'Table tool',
                  action: 'Excel download button clicked',
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
          </li>
          {pubMethodology?.methodology?.slug && (
            <li>
              <Link
                to="/methodology/[methodology]"
                as={`/methodology/${pubMethodology.methodology.slug}`}
              >
                Go to methodology
              </Link>
            </li>
          )}
          {pubMethodology?.externalMethodology?.url && (
            <li>
              <a href={pubMethodology.externalMethodology.url}>
                Go to methodology
              </a>
            </li>
          )}
        </ul>
      )}
      <p className="govuk-body">
        If you have a question about the data or methods used to create this
        table contact the named statistician via the relevant release page.
      </p>
    </div>
  );
};

export default memo(TableToolFinalStep);
