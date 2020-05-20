import ButtonText from '@common/components/ButtonText';
import LinkContainer from '@common/components/LinkContainer';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import DownloadCsvButton from '@common/modules/table-tool/components/DownloadCsvButton';
import DownloadExcelButton from '@common/modules/table-tool/components/DownloadExcelButton';
import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import TableToolWizard, {
  FinalStepRenderProps,
  TableToolWizardProps,
} from '@common/modules/table-tool/components/TableToolWizard';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import mapUnmappedTableHeaders from '@common/modules/table-tool/utils/mapUnmappedTableHeaders';
import permalinkService from '@common/services/permalinkService';
import publicationService from '@common/services/publicationService';
import { TableDataQuery } from '@common/services/tableBuilderService';
import { OmitStrict } from '@common/types';
import Link from '@frontend/components/Link';
import React, { useEffect, useRef, useState } from 'react';

interface TableToolFinalStepProps {
  publication: FinalStepRenderProps['publication'];
  query: TableDataQuery;
  table: FullTable;
  tableHeaders: TableHeadersConfig;
}

const TableToolFinalStep = ({
  table,
  tableHeaders,
  publication,
  query,
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

    const { id } = await permalinkService.createPermalink({
      query,
      configuration: {
        tableHeaders: mapUnmappedTableHeaders(currentTableHeaders),
      },
    });

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
              <div>Generated permanent link:</div>
              <LinkContainer
                url={`${window.location.host}/data-tables/permalink/${permalinkId}`}
                datatestid="permalink-generated-url"
              />
              <div>
                <Link
                  to="/data-tables/permalink/[permalink]"
                  as={`/data-tables/permalink/${permalinkId}`}
                  title="View created table permalink"
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  View permanent link
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
            <Link
              to="/find-statistics/[publication]"
              as={`/find-statistics/${publication.slug}`}
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
      )}
      <p className="govuk-body">
        If you have a question about the data or methods used to create this
        table contact the named statistician via the relevant release page.
      </p>
    </div>
  );
};

type TableToolProps = OmitStrict<TableToolWizardProps, 'finalStep'>;

const TableTool = (props: TableToolProps) => (
  <TableToolWizard
    {...props}
    finalStep={({ publication, query, response }) => (
      <WizardStep>
        {wizardStepProps => (
          <>
            <WizardStepHeading {...wizardStepProps}>
              Explore data
            </WizardStepHeading>

            {response && query && (
              <TableToolFinalStep
                publication={publication}
                query={query}
                table={response?.table}
                tableHeaders={response?.tableHeaders}
              />
            )}
          </>
        )}
      </WizardStep>
    )}
  />
);

export default TableTool;
