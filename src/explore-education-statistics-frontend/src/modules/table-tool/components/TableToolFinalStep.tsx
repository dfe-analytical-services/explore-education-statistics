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
import Details from '@common/components/Details';
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

  const { value: fullPublication } = useAsyncRetry(
    async () =>
      publicationService.getLatestPublicationRelease(selectedPublication.slug),
    [selectedPublication],
  );
  const publication = fullPublication?.publication;

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

  const handleCopyClick = () => {
    const el = document.querySelector(
      "[data-testid='permalink-generated-url']",
    ) as HTMLInputElement;
    el?.select();
    document.execCommand('copy');
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

      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-half">
          {table && (
            <Details summary="Additional options">
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
                {publication?.methodology?.slug && (
                  <li>
                    <Link
                      to="/methodology/[methodology]"
                      as={`/methodology/${publication.methodology.slug}`}
                    >
                      Go to methodology
                    </Link>
                  </li>
                )}
                {publication?.externalMethodology?.url && (
                  <li>
                    <a href={publication.externalMethodology.url}>
                      Go to methodology
                    </a>
                  </li>
                )}
              </ul>
            </Details>
          )}
          {publication?.contact && (
            <Details summary="Contact us">
              <p>
                If you have a question about the data or methods used to create
                this table contact the named statistician:
              </p>
              <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
                {publication?.contact.teamName}
              </h4>
              <p className="govuk-!-margin-top-0">
                Email <br />
                <a href={`mailto:${publication?.contact.teamEmail}`}>
                  {publication?.contact.teamEmail}
                </a>
              </p>
              <p>
                Telephone: {publication?.contact.contactName} <br />{' '}
                {publication?.contact.contactTelNo}
              </p>
            </Details>
          )}
        </div>

        <div className="govuk-grid-column-one-half dfe-align--right">
          {permalinkId ? (
            <div className="dfe-align--left">
              <h3 className="govuk-heading-s">Generated share link</h3>

              <div className="govuk-inset-text">
                Use the link below to see a version of this page that you can
                bookmark for future reference, or copy the link to send on to
                somebody else to view.
              </div>

              <p className="govuk-!-margin-top-0 govuk-!-margin-bottom-2">
                <UrlContainer
                  data-testid="permalink-generated-url"
                  url={`${window.location.origin}/data-tables/permalink/${permalinkId}`}
                />
              </p>

              <button
                type="button"
                className="govuk-button govuk-button--secondary govuk-!-margin-right-3"
                onClick={handleCopyClick}
              >
                Copy link
              </button>

              <Link
                className="govuk-!-margin-top-0 govuk-button"
                to="/data-tables/permalink/[permalink]"
                as={`/data-tables/permalink/${permalinkId}`}
                title="View created table permalink"
                target="_blank"
                rel="noopener noreferrer"
              >
                View share link
              </Link>
            </div>
          ) : (
            <LoadingSpinner
              alert
              inline
              loading={permalinkLoading}
              size="sm"
              text="Generating permanent link"
            >
              <ButtonText onClick={handlePermalinkClick}>
                Share your table
              </ButtonText>
            </LoadingSpinner>
          )}
        </div>
      </div>
    </div>
  );
};

export default memo(TableToolFinalStep);
