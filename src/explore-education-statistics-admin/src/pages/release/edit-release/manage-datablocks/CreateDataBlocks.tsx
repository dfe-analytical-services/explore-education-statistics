import DataBlockDetailsForm from '@admin/pages/release/edit-release/manage-datablocks/DataBlockDetailsForm';
import {
  DataBlock,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import React, { createRef } from 'react';
import TableToolWizard, {
  FinalStepProps,
  TableToolState,
} from '@common/modules/table-tool/components/TableToolWizard';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import TableHeadersForm, {
  TableHeadersFormValues,
} from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { reverseMapTableHeadersConfig } from '@common/modules/table-tool/components/utils/tableToolHelpers';
import getDefaultTableHeaderConfig from '@common/modules/full-table/utils/tableHeaders';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import { generateTableTitle } from '@common/modules/table-tool/components/DataTableCaption';

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
        onTableConfigurationChange={queryCompleted}
        finalStep={finalStepProps => (
          <WizardStep>
            {wizardStepProps => (
              <>
                <WizardStepHeading {...wizardStepProps}>
                  Configure data block
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
                    <div className="govuk-width-container">
                      <TimePeriodDataTable
                        ref={dataTableRef}
                        fullTable={finalStepProps.createdTable}
                        tableHeadersConfig={tableHeaders}
                      />
                    </div>
                  )}
                </div>

                {finalStepProps.query && tableHeaders && (
                  <DataBlockDetailsForm
                    initialValues={{
                      title:
                        finalStepProps && finalStepProps.createdTable
                          ? generateTableTitle(
                              finalStepProps.createdTable.subjectMeta,
                            )
                          : undefined,
                    }}
                    query={finalStepProps.query}
                    tableHeaders={tableHeaders}
                    initialDataBlock={dataBlock}
                    releaseId={releaseId}
                    onDataBlockSave={db => onDataBlockSave(db)}
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
