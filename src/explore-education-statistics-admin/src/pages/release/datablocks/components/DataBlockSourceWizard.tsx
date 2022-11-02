import DataBlockDetailsForm, {
  DataBlockDetailsFormValues,
} from '@admin/pages/release/datablocks/components/DataBlockDetailsForm';
import { ReleaseDataBlock } from '@admin/services/dataBlockService';
import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import TableToolWizard, {
  InitialTableToolState,
} from '@common/modules/table-tool/components/TableToolWizard';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import generateTableTitle from '@common/modules/table-tool/utils/generateTableTitle';
import { ReleaseTableDataQuery } from '@common/services/tableBuilderService';
import WarningMessage from '@common/components/WarningMessage';
import React, { createRef, memo, useCallback, useState } from 'react';

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
  onReorderTableHeaders: (reorderedTableHeaders: TableHeadersConfig) => void;
  onSave: DataBlockSourceWizardSaveHandler;
}

const DataBlockSourceWizardFinalStep = ({
  dataBlock,
  query,
  table,
  tableHeaders,
  onReorderTableHeaders,
  onSave,
}: DataBlockSourceWizardFinalStepProps) => {
  const dataTableRef = createRef<HTMLTableElement>();
  const [captionTitle, setCaptionTitle] = useState<string>(
    dataBlock?.heading ?? '',
  );

  const handleSubmit = useCallback(
    (details: DataBlockDetailsFormValues) => {
      onSave({
        details,
        table,
        tableHeaders,
        query,
      });
    },
    [onSave, query, table, tableHeaders],
  );

  return (
    <>
      <div className="govuk-!-margin-bottom-4">
        <TableHeadersForm
          initialValues={tableHeaders}
          onSubmit={nextTableHeaders => {
            onReorderTableHeaders(nextTableHeaders);
            if (dataTableRef.current) {
              // add a short delay so the reordering form is closed before it scrolls.
              setTimeout(() => {
                dataTableRef?.current?.scrollIntoView({
                  behavior: 'smooth',
                  block: 'start',
                });
              }, 200);
            }
          }}
        />

        <TimePeriodDataTable
          ref={dataTableRef}
          footnotesClassName="govuk-!-width-two-thirds"
          fullTable={table}
          captionTitle={captionTitle}
          query={query}
          tableHeadersConfig={tableHeaders}
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
      <h2 className="govuk-heading-m">Data source</h2>
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
        showTableQueryErrorDownload={false}
        finalStep={({ query, table, tableHeaders, onReorder }) => (
          <WizardStep size="l">
            {wizardStepProps => (
              <>
                <WizardStepHeading {...wizardStepProps}>
                  {dataBlock ? 'Update data block' : 'Create data block'}
                </WizardStepHeading>

                {query && table && tableHeaders && (
                  <DataBlockSourceWizardFinalStepWrapped
                    dataBlock={dataBlock}
                    query={query}
                    table={table}
                    tableHeaders={tableHeaders}
                    onReorderTableHeaders={onReorder}
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
