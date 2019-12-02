import ButtonText from '@common/components/ButtonText';
import LinkContainer from '@common/components/LinkContainer';
import LoadingSpinner from '@common/components/LoadingSpinner';
import permalinkService from '@common/modules/full-table/services/permalinkService';
import DownloadCsvButton from '@common/modules/table-tool/components/DownloadCsvButton';
import TableHeadersForm, {
  TableHeadersFormValues,
} from '@common/modules/table-tool/components/TableHeadersForm';
import TableToolWizard, {
  FinalStepProps,
  TableToolWizardProps,
} from '@common/modules/table-tool/components/TableToolWizard';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import Link from '@frontend/components/Link';
import React, { createRef, useEffect, useState } from 'react';

const TableToolFinalStep = ({
  table,
  tableHeaders: initialTableHeaders,
  publication,
  query,
}: FinalStepProps) => {
  const dataTableRef = createRef<HTMLTableElement>();
  const [permalinkId, setPermalinkId] = useState<string>('');
  const [permalinkLoading, setPermalinkLoading] = useState<boolean>(false);
  const [tableHeaders, setTableHeaders] = useState<TableHeadersFormValues>();

  useEffect(() => {
    setTableHeaders(initialTableHeaders);
  }, [initialTableHeaders]);

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
              initialValues={initialTableHeaders}
              onSubmit={tableHeaderConfig => {
                setTableHeaders(tableHeaderConfig);
                setPermalinkId('');
                if (dataTableRef.current) {
                  dataTableRef.current.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start',
                  });
                }
              }}
            />
            {table && tableHeaders && (
              <TimePeriodDataTable
                ref={dataTableRef}
                fullTable={table}
                tableHeadersConfig={tableHeaders}
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
                    Go to publication
                  </Link>
                </li>
                <li>
                  <DownloadCsvButton
                    publicationSlug={publication.slug}
                    fullTable={table}
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

type TableToolProps = Omit<
  TableToolWizardProps,
  'finalStep' | 'onTableConfigurationChange'
>;

const TableTool = (props: TableToolProps) => (
  <TableToolWizard {...props} finalStep={TableToolFinalStep} />
);

export default TableTool;
