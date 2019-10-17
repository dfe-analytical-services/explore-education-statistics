/* eslint-disable no-shadow */
import {
  PublicationSubjectMeta,
  TableDataQuery,
  ThemeMeta,
} from '@common/modules/full-table/services/tableBuilderService';
import { LocationFilter } from '@common/modules/full-table/types/filters';
import { FullTable } from '@common/modules/full-table/types/fullTable';
import TableHeadersForm, { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import TableToolWizard from '@common/modules/table-tool/components/TableToolWizard';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import { Dictionary, KeysRemap } from '@common/types/util';
import React, { createRef, ReactNode } from 'react';

import { DateRangeState } from './utils/tableToolHelpers';

interface Publication {
  id: string;
  title: string;
  slug: string;
}

interface FinalStepProps {
  publication?: Publication;
  createdTable: FullTable;
  query: TableDataQuery;
  tableHeaders: TableHeadersFormValues;
}

interface Props {
  themeMeta: ThemeMeta[];
  publicationId?: string;
  releaseId?: string;

  finalStepExtra?: (props: FinalStepProps) => ReactNode;
  finalStepHeading?: string;

  initialQuery?: TableDataQuery;
  initialTableHeaders?: TableHeadersFormValues;
  onInitialQueryCompleted?: () => void;
}


interface TableToolState {
  query: TableDataQuery | undefined;
  createdTable: FullTable | undefined;
  tableHeaders: TableHeadersFormValues | undefined;
  validInitialQuery: TableDataQuery | undefined;
  dateRange: DateRangeState;
  locations: Dictionary<LocationFilter[]>;
  subjectId: string;
  subjectMeta: PublicationSubjectMeta;
}


const TableTool = ( {
  themeMeta,
  publicationId,
  releaseId,
  finalStepExtra,
  finalStepHeading,
  initialQuery,
  initialTableHeaders,
  onInitialQueryCompleted,
} : Props) => {



  const dataTableRef = createRef<HTMLTableElement>();

  return (<TableToolWizard
    themeMeta={themeMeta}
    publicationId={publicationId}
    releaseId={releaseId}
    initialQuery={initialQuery}
    initialTableHeaders={initialTableHeaders}
    onInitialQueryCompleted={onInitialQueryCompleted}

    finalStep={stepProps => (
      <>
        <WizardStepHeading {...stepProps}>
          {finalStepHeading || 'Explore data'}
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

        {stepProps.createdTable &&
        stepProps.tableHeaders &&
        finalStepExtra &&
        stepProps.query &&
        finalStepExtra({
          createdTable: stepProps.createdTable,
          publication: stepProps.publication,
          tableHeaders: stepProps.tableHeaders,
          query: stepProps.query,
        })}
      </>
    )}
  />);



};

export default TableTool;
