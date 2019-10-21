import DataBlockDetailsForm from '@admin/pages/release/edit-release/manage-datablocks/DataBlockDetailsForm';
import { DataBlock } from '@admin/services/release/edit-release/datablocks/types';
import {
  DataBlockRequest,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import React, { createRef } from 'react';
import TableToolWizard, {
  FinalStepProps,
} from '@common/modules/table-tool/components/TableToolWizard';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import TableHeadersForm, {
  TableHeadersFormValues,
} from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { reverseMapTableHeadersConfig } from '@common/modules/table-tool/components/utils/tableToolHelpers';
import getDefaultTableHeaderConfig from '@common/modules/full-table/utils/tableHeaders';
import WizardStep from '@common/modules/table-tool/components/WizardStep';

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

  const initialTableHeaders = React.useMemo(() => {
    const configuredTableHeaders =
      (dataBlock &&
        dataBlock.tables &&
        dataBlock.tables.length > 0 && {
          ...dataBlock.tables[0].tableHeaders,
        }) ||
      undefined;

    return (
      (configuredTableHeaders &&
        configuredTableHeaders.rowGroups !== undefined &&
        configuredTableHeaders.columnGroups !== undefined &&
        configuredTableHeaders.columns !== undefined &&
        configuredTableHeaders.rows !== undefined &&
        configuredTableHeaders) ||
      undefined
    );
  }, [dataBlock]);

  const [tableHeaders, setTableHeaders] = React.useState<
    TableHeadersFormValues | undefined
  >(initialTableHeaders);

  React.useEffect(() => {
    setTableHeaders(initialTableHeaders);
  }, [initialTableHeaders]);

  const queryCompleted = React.useMemo(() => {
    const tableHeadersForCB =
      (dataBlock &&
        dataBlock.tables &&
        dataBlock.tables.length > 0 && {
          ...dataBlock.tables[0].tableHeaders,
        }) ||
      undefined;

    return (props: FinalStepProps) => {
      if (props.createdTable) {
        const headers =
          props.tableHeaders ||
          (tableHeadersForCB &&
            reverseMapTableHeadersConfig(
              tableHeadersForCB,
              props.createdTable.subjectMeta,
            )) ||
          getDefaultTableHeaderConfig(props.createdTable.subjectMeta);

        const tableHeadersConfig = {
          columnGroups: headers.columnGroups,
          columns: headers.columns.filter(_ => _ !== undefined),
          rowGroups: headers.rowGroups,
          rows: headers.rows.filter(_ => _ !== undefined),
        };

        if (
          tableHeadersConfig.columns.length === 0 ||
          tableHeadersConfig.rows.length === 0
        ) {
          // ignore this error
        } else {
          setTableHeaders(headers);
        }
      }

      if (onTableToolLoaded) onTableToolLoaded();
    };
  }, [dataBlock, onTableToolLoaded]);

  return (
    <div>
      <TableToolWizard
        releaseId={releaseId}
        themeMeta={[]}
        initialQuery={initialQuery}
        onTableConfigurationChange={queryCompleted}
        finalStep={finalStepProps => (
          <WizardStep>
            {wizardStepProps => (
              <>
                <WizardStepHeading {...wizardStepProps}>
                  Configure Data Block
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
                </div>

                {finalStepProps.query && tableHeaders && (
                  <DataBlockDetailsForm
                    query={finalStepProps.query}
                    tableHeaders={tableHeaders}
                    initialDataBlock={dataBlock}
                    releaseId={releaseId}
                    onDataBlockSave={onDataBlockSave}
                  />
                )}
              </>
            )}
          </WizardStep>
        )}
      />
    </div>
  );
};

export default CreateDataBlocks;
