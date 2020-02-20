import ButtonText from '@common/components/ButtonText';
import LinkContainer from '@common/components/LinkContainer';
import LoadingSpinner from '@common/components/LoadingSpinner';
import DownloadCsvButton from '@common/modules/table-tool/components/DownloadCsvButton';
import DownloadExcelButton from '@common/modules/table-tool/components/DownloadExcelButton';
import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import TableToolWizard, {
  FinalStepProps,
  TableToolWizardProps,
} from '@common/modules/table-tool/components/TableToolWizard';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import permalinkService from '@common/modules/table-tool/services/permalinkService';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/utils/tableHeaders';
import { OmitStrict } from '@common/types';
import Link from '@frontend/components/Link';
import React, { useEffect, useRef, useState } from 'react';

const TableToolFinalStep = ({
  table,
  tableHeaders,
  publication,
  query,
}: FinalStepProps) => {
  const dataTableRef = useRef<HTMLElement>(null);
  const [permalinkId, setPermalinkId] = useState<string>('');
  const [permalinkLoading, setPermalinkLoading] = useState<boolean>(false);
  const [currentTable, setCurrentTable] = useState<FullTable>();
  const [currentTableHeaders, setCurrentTableHeaders] = useState<
    TableHeadersConfig
  >();

  useEffect(() => {
    // The current table is stored to ensure the headers
    // and table only render together as a matching pair
    setCurrentTable(table);
    setCurrentTableHeaders(tableHeaders);
  }, [tableHeaders, table]);

  const handlePermalinkClick = async () => {
    if (!tableHeaders || !query) {
      return;
    }
    setPermalinkLoading(true);

    const { id } = await permalinkService.createTablePermalink({
      ...query,
      configuration: {
        tableHeadersConfig: tableHeaders,
      },
    });

    setPermalinkId(id);
    setPermalinkLoading(false);
  };

  return (
    <WizardStep>
      {wizardStepProps => (
        <>
          <WizardStepHeading {...wizardStepProps}>
            Explore data
          </WizardStepHeading>

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
            {currentTable && currentTableHeaders && (
              <TimePeriodDataTable
                ref={dataTableRef}
                fullTable={currentTable}
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
                    />
                    <div>
                      <a
                        className="govuk-link"
                        href={`/data-tables/permalink/${permalinkId}`}
                        title="View created table permalink"
                        target="_blank"
                        rel="noopener noreferrer"
                      >
                        View permanent link
                      </a>
                    </div>
                  </>
                ) : (
                  <>
                    {permalinkLoading ? (
                      <LoadingSpinner
                        inline
                        size={19}
                        text="Generating permanent link"
                      />
                    ) : (
                      <ButtonText onClick={() => handlePermalinkClick()}>
                        Generate permanent link
                      </ButtonText>
                    )}
                  </>
                )}
              </li>
            </ul>

            <h3>Additional options</h3>
            {publication && table && (
              <ul className="govuk-list">
                <li>
                  <Link
                    as={`/find-statistics/${publication.slug}`}
                    to={`/find-statistics/publication?publication=${publication.slug}`}
                  >
                    View the release for this data
                  </Link>
                </li>
                <li>
                  <DownloadCsvButton
                    publicationSlug={publication.slug}
                    fullTable={table}
                  />
                </li>
                <li>
                  <DownloadExcelButton
                    publicationSlug={publication.slug}
                    tableRef={dataTableRef}
                    footnotes={table?.subjectMeta.footnotes}
                  />
                </li>

                <li>
                  <a href="#api">Access developer API</a>
                </li>
                <li>
                  <Link
                    as={`/methodology/${publication.slug}`}
                    to={`/methodology/methodology?publication=${publication.slug}`}
                  >
                    Go to methodology
                  </Link>
                </li>
              </ul>
            )}
            <p className="govuk-body">
              If you have a question about the data or methods used to create
              this table contact the named statistician via the relevant release
              page.
            </p>
          </div>
        </>
      )}
    </WizardStep>
  );
};

type TableToolProps = OmitStrict<TableToolWizardProps, 'finalStep'>;

const TableTool = (props: TableToolProps) => (
  <TableToolWizard {...props} finalStep={TableToolFinalStep} />
);

export default TableTool;
