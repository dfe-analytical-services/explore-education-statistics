import DataBlockDetailsForm, {
  DataBlockDetailsFormValues,
} from '@admin/pages/release/edit-release/manage-datablocks/components/DataBlockDetailsForm';
import {
  CreateReleaseDataBlock,
  ReleaseDataBlock,
} from '@admin/services/release/edit-release/datablocks/service';
import { generateTableTitle } from '@common/modules/table-tool/components/DataTableCaption';
import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import TableToolWizard, {
  TableToolState,
} from '@common/modules/table-tool/components/TableToolWizard';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import mapUnmappedTableHeaders from '@common/modules/table-tool/utils/mapUnmappedTableHeaders';
import { TableDataQuery } from '@common/services/tableBuilderService';
import React, { createRef, useCallback, useEffect, useState } from 'react';

export type SavedDataBlock = CreateReleaseDataBlock & {
  id?: string;
};

interface DataBlockSourceWizardFinalStepProps {
  dataBlock?: ReleaseDataBlock;
  query: TableDataQuery;
  table: FullTable;
  tableHeaders: TableHeadersConfig;
  onDataBlockSave: (dataBlock: SavedDataBlock) => void;
}

const DataBlockSourceWizardFinalStep = ({
  dataBlock,
  query,
  table,
  tableHeaders,
  onDataBlockSave,
}: DataBlockSourceWizardFinalStepProps) => {
  const dataTableRef = createRef<HTMLTableElement>();

  const [currentTableHeaders, setCurrentTableHeaders] = useState<
    TableHeadersConfig
  >(tableHeaders);

  const [captionTitle, setCaptionTitle] = useState<string>(
    dataBlock?.heading ?? '',
  );

  useEffect(() => {
    // Synchronize table headers if the table
    // has been changed by the user.
    setCurrentTableHeaders(tableHeaders);
  }, [tableHeaders]);

  const handleSubmit = useCallback(
    (values: DataBlockDetailsFormValues) => {
      onDataBlockSave({
        charts: [],
        ...(dataBlock ?? {}),
        ...values,
        dataBlockRequest: query,
        tables: [
          {
            tableHeaders: mapUnmappedTableHeaders(currentTableHeaders),
            indicators: [],
          },
        ],
      });
    },
    [currentTableHeaders, dataBlock, onDataBlockSave, query],
  );

  return (
    <>
      <div className="govuk-!-margin-bottom-4">
        <TableHeadersForm
          initialValues={currentTableHeaders}
          id="dataBlockSourceWizard-tableHeadersForm"
          onSubmit={async nextTableHeaders => {
            setCurrentTableHeaders(nextTableHeaders);

            if (dataTableRef.current) {
              dataTableRef.current.scrollIntoView({
                behavior: 'smooth',
                block: 'start',
              });
            }
          }}
        />

        <TimePeriodDataTable
          ref={dataTableRef}
          fullTable={table}
          captionTitle={captionTitle}
          tableHeadersConfig={currentTableHeaders}
        />
      </div>

      <hr />

      <DataBlockDetailsForm
        initialValues={{
          heading: dataBlock?.heading ?? generateTableTitle(table.subjectMeta),
          name: dataBlock?.name ?? '',
          source: dataBlock?.source ?? '',
        }}
        onTitleChange={setCaptionTitle}
        onSubmit={handleSubmit}
      />
    </>
  );
};

interface DataBlockSourceWizardProps {
  releaseId: string;
  dataBlock?: ReleaseDataBlock;
  initialTableToolState?: TableToolState;
  onDataBlockSave: (dataBlock: SavedDataBlock) => void;
}

const DataBlockSourceWizard = ({
  releaseId,
  dataBlock,
  initialTableToolState,
  onDataBlockSave,
}: DataBlockSourceWizardProps) => {
  return (
    <>
      <p>Configure data source for the data block</p>

      <TableToolWizard
        releaseId={releaseId}
        themeMeta={[]}
        initialState={initialTableToolState}
        finalStep={({ response, query }) => (
          <WizardStep>
            {wizardStepProps => (
              <>
                <WizardStepHeading {...wizardStepProps}>
                  {dataBlock ? 'Update data block' : 'Create data block'}
                </WizardStepHeading>

                {query && response && (
                  <DataBlockSourceWizardFinalStep
                    dataBlock={dataBlock}
                    query={query}
                    table={response.table}
                    tableHeaders={response.tableHeaders}
                    onDataBlockSave={onDataBlockSave}
                  />
                )}
              </>
            )}
          </WizardStep>
        )}
      />
    </>
  );
};

export default DataBlockSourceWizard;
