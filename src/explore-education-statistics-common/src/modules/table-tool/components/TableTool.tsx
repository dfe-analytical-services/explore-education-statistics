/* eslint-disable no-shadow */
import {
  TableDataQuery,
  ThemeMeta,
} from '@common/modules/full-table/services/tableBuilderService';
import getDefaultTableHeaderConfig from '@common/modules/full-table/utils/tableHeaders';
import TableHeadersForm, {
  TableHeadersFormValues,
} from '@common/modules/table-tool/components/TableHeadersForm';
import TableToolWizard, {
  FinalStepProps,
} from '@common/modules/table-tool/components/TableToolWizard';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { reverseMapTableHeadersConfig } from '@common/modules/table-tool/components/utils/tableToolHelpers';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import React, { createRef, ReactNode } from 'react';
import WizardStep from '@common/modules/table-tool/components/WizardStep';

interface Publication {
  id: string;
  title: string;
  slug: string;
}

interface Props {
  themeMeta: ThemeMeta[];
  publicationId?: string;
  releaseId?: string;
  initialQuery?: TableDataQuery;
  initialTableHeaders?: TableHeadersFormValues;
  onInitialQueryCompleted?: () => void;

  finalStepExtra?: (props: FinalStepProps) => ReactNode;
  finalStepHeading?: string;
}

const TableTool = ({
  themeMeta,
  publicationId,
  releaseId,
  initialQuery,
  initialTableHeaders,
  onInitialQueryCompleted,
  finalStepExtra,
  finalStepHeading,
}: Props) => {
  const dataTableRef = createRef<HTMLTableElement>();

  const [tableHeaders, setTableHeaders] = React.useState<
    TableHeadersFormValues | undefined
  >(initialTableHeaders);

  React.useEffect(() => {
    setTableHeaders(initialTableHeaders);
  }, [initialTableHeaders]);

  return (
    <TableToolWizard
      themeMeta={themeMeta}
      publicationId={publicationId}
      releaseId={releaseId}
      initialQuery={initialQuery}
      onTableConfigurationChange={props => {
        if (props.createdTable) {
          setTableHeaders(
            (tableHeaders &&
              reverseMapTableHeadersConfig(
                tableHeaders,
                props.createdTable.subjectMeta,
              )) ||
              getDefaultTableHeaderConfig(props.createdTable.subjectMeta),
          );
        }

        if (onInitialQueryCompleted) onInitialQueryCompleted();
      }}
      finalStep={finalStepProps => (
        <WizardStep>
          {wizardStepProps => (
            <>
              <WizardStepHeading {...wizardStepProps}>
                {finalStepHeading || 'Explore data'}
              </WizardStepHeading>

              <div className="govuk-!-margin-bottom-4">
                <TableHeadersForm
                  initialValues={tableHeaders}
                  onSubmit={tableHeaderConfig => {
                    setTableHeaders(tableHeaderConfig);

                    if (dataTableRef.current) {
                      dataTableRef.current.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start',
                      });
                    }
                  }}
                />
                {finalStepProps.createdTable && tableHeaders && (
                  <TimePeriodDataTable
                    ref={dataTableRef}
                    fullTable={finalStepProps.createdTable}
                    tableHeadersConfig={tableHeaders}
                  />
                )}

                {finalStepProps.createdTable &&
                  tableHeaders &&
                  finalStepExtra &&
                  finalStepProps.query &&
                  finalStepExtra({
                    createdTable: finalStepProps.createdTable,
                    publication: finalStepProps.publication,
                    tableHeaders,
                    query: finalStepProps.query,
                  })}
              </div>
            </>
          )}
        </WizardStep>
      )}
    />
  );
};

export default TableTool;
