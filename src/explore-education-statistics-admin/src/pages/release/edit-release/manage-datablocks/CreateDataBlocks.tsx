import DataBlockDetailsForm from '@admin/pages/release/edit-release/manage-datablocks/DataBlockDetailsForm';
import getDefaultTableHeaderConfig from '@common/modules/full-table/utils/tableHeaders';
import { generateTableTitle } from '@common/modules/table-tool/components/DataTableCaption';
import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import TableToolWizard, {
  FinalStepProps,
  TableToolState,
} from '@common/modules/table-tool/components/TableToolWizard';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { reverseMapTableHeadersConfig } from '@common/modules/table-tool/components/utils/tableToolHelpers';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import {
  DataBlock,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import React, { createRef } from 'react';

interface Props {
  releaseId: string;
  dataBlock?: DataBlock;
  dataBlockResponse?: DataBlockResponse;
  onDataBlockSave: (
    dataBlock: DataBlock,
    newDataBlockResponse?: TableToolState,
  ) => Promise<DataBlock>;
  onTableToolLoaded?: () => void;
}

const CreateDataBlocks = ({
  releaseId,
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
      if (props.table) {
        const headers =
          props.tableHeaders ||
          (tableHeadersForCB &&
            reverseMapTableHeadersConfig(
              tableHeadersForCB,
              props.table.subjectMeta,
            )) ||
          getDefaultTableHeaderConfig(props.table.subjectMeta);

        const tableHeadersConfig = {
          columnGroups: headers.columnGroups,
          columns: headers.columns.filter(_ => _ !== undefined),
          rowGroups: headers.rowGroups,
          rows: headers.rows.filter(_ => _ !== undefined),
        };

        if (
          tableHeadersConfig.columns.length !== 0 &&
          tableHeadersConfig.rows.length !== 0
        ) {
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
        initialQuery={dataBlock ? dataBlock.dataBlockRequest : undefined}
        finalStep={finalStepProps => (
          <WizardStep>
            {wizardStepProps => (
              <>
                <WizardStepHeading {...wizardStepProps}>
                  Data block details
                </WizardStepHeading>

                {finalStepProps.query && tableHeaders && (
                  <DataBlockDetailsForm
                    initialValues={{
                      title:
                        finalStepProps && finalStepProps.table
                          ? generateTableTitle(finalStepProps.table.subjectMeta)
                          : undefined,
                    }}
                    query={finalStepProps.query}
                    tableHeaders={tableHeaders}
                    initialDataBlock={dataBlock}
                    releaseId={releaseId}
                    onDataBlockSave={db => onDataBlockSave(db)}
                  >
                    {finalStepProps.table && tableHeaders && (
                      <div className="govuk-!-margin-bottom-4">
                        <div className="govuk-width-container">
                          <TimePeriodDataTable
                            ref={dataTableRef}
                            fullTable={finalStepProps.table}
                            tableHeadersConfig={tableHeaders}
                          />
                        </div>
                      </div>
                    )}
                  </DataBlockDetailsForm>
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
