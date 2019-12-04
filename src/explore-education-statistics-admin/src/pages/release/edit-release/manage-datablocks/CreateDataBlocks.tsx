import DataBlockDetailsForm from '@admin/pages/release/edit-release/manage-datablocks/DataBlockDetailsForm';
import getDefaultTableHeaderConfig from '@common/modules/full-table/utils/tableHeaders';
import { generateTableTitle } from '@common/modules/table-tool/components/DataTableCaption';
import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import TableToolWizard, {
  TableToolState,
} from '@common/modules/table-tool/components/TableToolWizard';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import {
  DataBlock,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import React, { createRef, useState, useEffect } from 'react';
import { TableDataQuery } from '@common/modules/full-table/services/tableBuilderService';
import { FullTable } from '@common/modules/full-table/types/fullTable';
import { mapDataBlockResponseToFullTable } from '@common/modules/find-statistics/components/util/tableUtil';

interface CreateDataBlockProps {
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
  dataBlockResponse,
  onTableToolLoaded,
}: CreateDataBlockProps) => {
  const dataTableRef = createRef<HTMLTableElement>();

  const [query, setQuery] = useState<TableDataQuery | undefined>(
    (dataBlock && dataBlock.dataBlockRequest) || undefined,
  );
  const [table, setTable] = useState<FullTable | undefined>(
    dataBlockResponse && mapDataBlockResponseToFullTable(dataBlockResponse),
  );

  const [tableHeaders, setTableHeaders] = useState<
    TableHeadersFormValues | undefined
  >();

  useEffect(() => {
    if (dataBlock && dataBlockResponse) {
      setQuery(dataBlock.dataBlockRequest);
      const dataTable = mapDataBlockResponseToFullTable(dataBlockResponse);
      setTable(dataTable);

      const firstTableConfig = dataBlock &&
        dataBlock.tables &&
        dataBlock.tables.length > 0 && {
          ...dataBlock.tables[0].tableHeaders,
        };
      if (firstTableConfig) {
        setTableHeaders(firstTableConfig);
      } else {
        setTableHeaders(
          getDefaultTableHeaderConfig(
            mapDataBlockResponseToFullTable(dataBlockResponse).subjectMeta,
          ),
        );
      }
    }
    if (onTableToolLoaded) onTableToolLoaded();
  }, [dataBlock, dataBlockResponse]);

  const getInitialValues = () => {
    if (!dataBlock) {
      return {
        title: table ? generateTableTitle(table.subjectMeta) : undefined,
      };
    }

    const { heading: title, name, source, customFootnotes } = dataBlock;
    return {
      title,
      name,
      source,
      customFootnotes,
    };
  };

  return (
    <div>
      <TableToolWizard
        releaseId={releaseId}
        themeMeta={[]}
        initialQuery={dataBlock ? dataBlock.dataBlockRequest : undefined}
        onTableCreated={response => {
          setQuery(response.query);
          setTable(response.table);
          setTableHeaders(response.tableHeaders);
        }}
        finalStep={() => (
          <WizardStep>
            {wizardStepProps => (
              <>
                <WizardStepHeading {...wizardStepProps}>
                  Data block details
                </WizardStepHeading>

                {query && tableHeaders && (
                  <DataBlockDetailsForm
                    initialValues={getInitialValues()}
                    query={query}
                    tableHeaders={tableHeaders || tableHeaders}
                    initialDataBlock={dataBlock}
                    releaseId={releaseId}
                    onDataBlockSave={db => onDataBlockSave(db)}
                  >
                    {table && tableHeaders && (
                      <div className="govuk-!-margin-bottom-4">
                        <div className="govuk-width-container">
                          <TimePeriodDataTable
                            ref={dataTableRef}
                            fullTable={table}
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
