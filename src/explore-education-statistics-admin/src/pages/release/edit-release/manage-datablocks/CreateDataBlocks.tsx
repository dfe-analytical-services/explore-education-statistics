import DataBlockDetailsForm from '@admin/pages/release/edit-release/manage-datablocks/DataBlockDetailsForm';
import { DataBlock } from '@admin/services/release/edit-release/datablocks/types';
import TableTool from '@common/modules/table-tool/components/TableTool';
import {
  DataBlockRequest,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import React, { createRef } from 'react';
import TableToolWizard from '@common/modules/table-tool/components/TableToolWizard';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import TableHeadersForm, {
  TableHeadersFormValues,
} from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { reverseMapTableHeadersConfig } from '@common/modules/table-tool/components/utils/tableToolHelpers';
import getDefaultTableHeaderConfig from '@common/modules/full-table/utils/tableHeaders';

interface Props {
  releaseId: string;
  dataBlockRequest?: DataBlockRequest;
  dataBlockResponse?: DataBlockResponse;
  dataBlock?: DataBlock;

  onDataBlockSave: (dataBlock: DataBlock) => Promise<DataBlock>;
  onTableToolLoaded?: () => void;
}

const CreateDataBlocks = ({
  releaseId,
  dataBlockRequest: initialQuery,
  dataBlock,
  onDataBlockSave,
  onTableToolLoaded,
}: Props) => {
  const dataTableRef = createRef<HTMLTableElement>();

  const initialTableHeaders = React.useMemo(
    () =>
      (dataBlock &&
        dataBlock.tables &&
        dataBlock.tables.length > 0 && {
          ...dataBlock.tables[0].tableHeaders,
        }) ||
      undefined,
    [dataBlock],
  );

  const [tableHeaders, setTableHeaders] = React.useState<
    TableHeadersFormValues | undefined
  >(initialTableHeaders);

  React.useEffect(() => {
    setTableHeaders(initialTableHeaders);
  }, [initialTableHeaders]);

  return (
    <div>
      {releaseId !== undefined && (
        <TableToolWizard
          releaseId={releaseId}
          themeMeta={[]}
          initialQuery={initialQuery}
          initialTableHeaders={tableHeaders}
          onInitialQueryCompleted={props => {
            if (props.createdTable) {
              setTableHeaders(
                (props.tableHeaders &&
                  reverseMapTableHeadersConfig(
                    props.tableHeaders,
                    props.createdTable.subjectMeta,
                  )) ||
                  getDefaultTableHeaderConfig(props.createdTable.subjectMeta),
              );
            }

            if (onTableToolLoaded) onTableToolLoaded();
          }}
          finalStep={stepProps => (
            <>
              <WizardStepHeading {...stepProps}>
                Configure Data Block
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

              {stepProps.query && stepProps.tableHeaders && (
                <DataBlockDetailsForm
                  query={stepProps.query}
                  tableHeaders={stepProps.tableHeaders}
                  initialDataBlock={dataBlock}
                  releaseId={releaseId}
                  onDataBlockSave={onDataBlockSave}
                />
              )}
            </>
          )}
        />
      )}
    </div>
  );
};

export default CreateDataBlocks;
