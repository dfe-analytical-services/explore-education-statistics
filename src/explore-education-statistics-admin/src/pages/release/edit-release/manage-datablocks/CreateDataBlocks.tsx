import DataBlockDetailsForm, {
  DataBlockDetailsFormValues,
} from '@admin/pages/release/edit-release/manage-datablocks/DataBlockDetailsForm';
import { mapDataBlockResponseToFullTable } from '@common/modules/find-statistics/components/util/tableUtil';
import { TableDataQuery } from '@common/modules/full-table/services/tableBuilderService';
import { FullTable } from '@common/modules/full-table/types/fullTable';
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
import React, { createRef, useEffect, useState } from 'react';

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

  const [initialValues, setInitialValues] = useState<
    DataBlockDetailsFormValues
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
  }, [dataBlock, dataBlockResponse]);

  useEffect(() => {
    if (!dataBlock) {
      setInitialValues({
        title: table ? generateTableTitle(table.subjectMeta) : '',
        name: '',
        source: '',
        customFootnotes: '',
      });
      return;
    }

    const {
      heading: title = '',
      name = '',
      source = '',
      customFootnotes = '',
    } = dataBlock;

    setInitialValues({
      title,
      name,
      source,
      customFootnotes,
    });
  }, [dataBlock, table]);

  return (
    <div>
      <TableToolWizard
        releaseId={releaseId}
        themeMeta={[]}
        initialQuery={dataBlock ? dataBlock.dataBlockRequest : undefined}
        onInitialQueryLoaded={() => {
          if (onTableToolLoaded) onTableToolLoaded();
        }}
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
                    initialValues={initialValues}
                    query={query}
                    tableHeaders={tableHeaders}
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
