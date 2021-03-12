import ButtonText from '@common/components/ButtonText';
import Details from '@common/components/Details';
import UrlContainer from '@common/components/UrlContainer';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import { FinalStepRenderProps } from '@common/modules/table-tool/components/TableToolWizard';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import mapUnmappedTableHeaders from '@common/modules/table-tool/utils/mapUnmappedTableHeaders';
import permalinkService from '@common/services/permalinkService';
import publicationService from '@common/services/publicationService';
import { TableDataQuery } from '@common/services/tableBuilderService';
import Link from '@frontend/components/Link';
import React, { memo, useEffect, useRef, useState } from 'react';
import DownloadCsvButton from '@common/modules/table-tool/components/DownloadCsvButton';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import DownloadExcelButton from '@common/modules/table-tool/components/DownloadExcelButton';
// import { Details } from 'govuk-frontend';

interface TableToolFinalStepProps {
  publication: FinalStepRenderProps['publication'];
  query: TableDataQuery;
  table: FullTable;
  tableHeaders: TableHeadersConfig;
  releaseId?: string;
}

const TableToolFinalStep = ({
  table,
  tableHeaders,
  publication,
  query,
  releaseId,
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

  const { value: pubMethodology } = useAsyncRetry(async () => {
    if (publication) {
      return publicationService.getPublicationMethodology(publication.slug);
    }
    return undefined;
  }, [publication]);

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
      releaseId,
    );

    setPermalinkId(id);
    setPermalinkLoading(false);
  };

  return (
    <div className="govuk-!-margin-bottom-4">
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
        <TimePeriodDataTable
          ref={dataTableRef}
          fullTable={table}
          tableHeadersConfig={currentTableHeaders}
        />
      )}
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-half">
          {publication && table && (
            <Details summary="Download data">
              <ul className="govuk-list">
                <li>
                  <DownloadCsvButton
                    fileName={`data-${publication.slug}`}
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
                    fileName={`data-${publication.slug}`}
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
                <li>
                  {pubMethodology?.methodology?.slug && (
                    <Link
                      to="/methodology/[methodology]"
                      as={`/methodology/${pubMethodology.methodology.slug}`}
                    >
                      Go to methodology
                    </Link>
                  )}
                  {pubMethodology?.externalMethodology?.url && (
                    <a href={pubMethodology.externalMethodology.url}>
                      Go to methodology
                    </a>
                  )}
                </li>
              </ul>
            </Details>
          )}
        </div>
        <div className="govuk-grid-column-one-half dfe-align--right">
          {permalinkId ? (
            <>
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

                <a
                  href="#"
                  className="govuk-button govuk-button--secondary govuk-!-margin-right-3"
                >
                  Copy link
                </a>

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
            </>
          ) : (
            <LoadingSpinner
              alert
              inline
              loading={permalinkLoading}
              size="sm"
              text="Generating permanent link"
            >
              <ButtonText onClick={() => handlePermalinkClick()}>
                Share your table
              </ButtonText>
            </LoadingSpinner>
          )}
        </div>
      </div>
      <div className="govuk-inset-text">
        <p className="govuk-body">
          If you have a question about the data or methods used to create this
          table contact the named statistician via the relevant release page.{' '}
          <br />
          {publication && table && (
            <Link
              to="/find-statistics/[publication]"
              as={`/find-statistics/${publication.slug}`}
            >
              View the release for this data
            </Link>
          )}
        </p>
      </div>
    </div>
  );
};

export default memo(TableToolFinalStep);
