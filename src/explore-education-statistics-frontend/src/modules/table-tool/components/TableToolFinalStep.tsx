import ButtonText from '@common/components/ButtonText';
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

interface TableToolFinalStepProps {
  publication: FinalStepRenderProps['publication'];
  query: TableDataQuery;
  table: FullTable;
  tableHeaders: TableHeadersConfig;
  releaseId?: string;
  releaseSlug?: string;
}

const TableToolFinalStep = ({
  table,
  tableHeaders,
  publication,
  query,
  releaseId,
  releaseSlug,
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
              <ButtonText onClick={() => handlePermalinkClick()}>
                Generate permanent link
              </ButtonText>
            </LoadingSpinner>
          )}
        </li>
      </ul>

      <h3>Additional options</h3>
      {publication && table && (
        <ul className="govuk-list">
          <li>
            {releaseSlug ? (
              <Link
                to="/find-statistics/[publication]/[releaseSlug]"
                as={`/find-statistics/${publication.slug}/${releaseSlug}`}
              >
                View the release for this data
              </Link>
            ) : (
              <Link
                to="/find-statistics/[publication]"
                as={`/find-statistics/${publication.slug}`}
              >
                View the release for this data
              </Link>
            )}
          </li>
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
