import DataBlockDetailsForm, {
  DataBlockDetailsFormValues,
} from '@admin/pages/release/datablocks/components/DataBlockDetailsForm';
import { ReleaseDataBlock } from '@admin/services/dataBlockService';
import { generateTableTitle } from '@common/modules/table-tool/components/DataTableCaption';
import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import TableToolWizard, {
  InitialTableToolState,
} from '@common/modules/table-tool/components/TableToolWizard';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import { ReleaseTableDataQuery } from '@common/services/tableBuilderService';
import WarningMessage from '@common/components/WarningMessage';
import React, {
  createRef,
  memo,
  useCallback,
  useEffect,
  useState,
} from 'react';

export type DataBlockSourceWizardSaveHandler = (params: {
  details: DataBlockDetailsFormValues;
  table: FullTable;
  tableHeaders: TableHeadersConfig;
  query: ReleaseTableDataQuery;
}) => void;

interface DataBlockSourceWizardFinalStepProps {
  dataBlock?: ReleaseDataBlock;
  query: ReleaseTableDataQuery;
  table: FullTable;
  tableHeaders: TableHeadersConfig;
  onSave: DataBlockSourceWizardSaveHandler;
}

const DataBlockSourceWizardFinalStep = ({
  dataBlock,
  query,
  table,
  tableHeaders,
  onSave,
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
    (details: DataBlockDetailsFormValues) => {
      onSave({
        details,
        table,
        tableHeaders: currentTableHeaders,
        query,
      });
    },
    [currentTableHeaders, onSave, query, table],
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

      <DataBlockDetailsForm
        initialValues={{
          heading: dataBlock?.heading ?? generateTableTitle(table.subjectMeta),
          highlightName: dataBlock?.highlightName ?? '',
          highlightDescription: dataBlock?.highlightDescription ?? '',
          name: dataBlock?.name ?? '',
          source: dataBlock?.source ?? '',
        }}
        onTitleChange={setCaptionTitle}
        onSubmit={handleSubmit}
      />
    </>
  );
};

const DataBlockSourceWizardFinalStepWrapped = memo(
  DataBlockSourceWizardFinalStep,
);

interface DataBlockSourceWizardProps {
  dataBlock?: ReleaseDataBlock;
  tableToolState: InitialTableToolState;
  onSave: DataBlockSourceWizardSaveHandler;
}

const DataBlockSourceWizard = ({
  dataBlock,
  tableToolState,
  onSave,
}: DataBlockSourceWizardProps) => {
  return (
    <div className="govuk-!-margin-bottom-8">
      <p>Configure data source for the data block</p>
      {dataBlock && dataBlock.charts.length > 0 && (
        <WarningMessage>
          Editing the data source may change existing chart configuration for
          this data block
        </WarningMessage>
      )}

      <TableToolWizard
        themeMeta={[]}
        hidePublicationSelectionStage
        initialState={tableToolState}
        tableSizeErrorDownloadAvailable={false}
        finalStep={({ response, query }) => (
          <WizardStep size="l">
            {wizardStepProps => (
              <>
                <WizardStepHeading {...wizardStepProps}>
                  {dataBlock ? 'Update data block' : 'Create data block'}
                </WizardStepHeading>

                {query && response && (
                  <DataBlockSourceWizardFinalStepWrapped
                    dataBlock={dataBlock}
                    query={query}
                    table={response.table}
                    tableHeaders={response.tableHeaders}
                    onSave={onSave}
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

export default DataBlockSourceWizard;
