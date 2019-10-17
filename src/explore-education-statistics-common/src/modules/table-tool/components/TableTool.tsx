/* eslint-disable no-shadow */
import { TableDataQuery, ThemeMeta } from '@common/modules/full-table/services/tableBuilderService';
import TableHeadersForm, { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import TableToolWizard from '@common/modules/table-tool/components/TableToolWizard';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import React, { createRef } from 'react';

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
}

const TableTool = ( {
  themeMeta,
  publicationId,
  releaseId,
  initialQuery,
  initialTableHeaders,
  onInitialQueryCompleted,
} : Props) => {



  const dataTableRef = createRef<HTMLTableElement>();

  return (<TableToolWizard
    {...props}
    themeMeta={themeMeta}
    publicationId={publicationId}
    releaseId={releaseId}
    initialQuery={initialQuery}
    initialTableHeaders={initialTableHeaders}
    onInitialQueryCompleted={onInitialQueryCompleted}

    finalStep={stepProps => (
      <>
        <WizardStepHeading {...stepProps}>
          Explore data
        </WizardStepHeading>

        <div className="govuk-!-margin-bottom-4">
          <TableHeadersForm
            initialValues={stepProps.tableHeaders}
            onSubmit={tableHeaderConfig => {
              stepProps.updateState({
                tableHeaders: tableHeaderConfig,
              });

              if (dataTableRef.current) {
                dataTableRef.current.scrollIntoView({
                  behavior: 'smooth',
                  block: 'start',
                });
              }
            }}
          />
          {stepProps.createdTable && stepProps.tableHeaders && (
            <TimePeriodDataTable
              ref={dataTableRef}
              fullTable={stepProps.createdTable}
              tableHeadersConfig={stepProps.tableHeaders}
            />
          )}
        </div>
      </>
    )}
  />);



};

export default TableTool;
