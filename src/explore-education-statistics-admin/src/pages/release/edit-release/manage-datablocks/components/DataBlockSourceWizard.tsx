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
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import { TableDataQuery } from '@common/services/tableBuilderService';
import React, { createRef, useCallback, useState } from 'react';

export type SavedDataBlock = CreateReleaseDataBlock & {
  id?: string;
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
  const dataTableRef = createRef<HTMLTableElement>();

  const [query, setQuery] = useState<TableDataQuery | undefined>(
    dataBlock?.dataBlockRequest,
  );
  const [tableHeaders, setTableHeaders] = useState<
    TableHeadersConfig | undefined
  >(initialTableToolState?.response?.tableHeaders);

  const [captionTitle, setCaptionTitle] = useState<string>(
    dataBlock?.heading ?? '',
  );

  const handleSubmit = useCallback(
    (values: DataBlockDetailsFormValues) => {
      if (!query || !tableHeaders) {
        return;
      }

      const savedDataBlock: SavedDataBlock = {
        charts: [],
        tables: [],
        ...(dataBlock ?? {}),
        ...values,
        dataBlockRequest: {
          ...query,
          geographicLevel: query.geographicLevel,
          timePeriod: query.timePeriod && {
            ...query.timePeriod,
            startCode: query.timePeriod.startCode,
            endCode: query.timePeriod.endCode,
          },
        },
      };

      onDataBlockSave({
        ...savedDataBlock,
        tables: [
          {
            tableHeaders,
            indicators: [],
          },
        ],
      });
    },
    [dataBlock, onDataBlockSave, query, tableHeaders],
  );

  return (
    <TableToolWizard
      releaseId={releaseId}
      themeMeta={[]}
      initialState={initialTableToolState}
      onTableCreated={response => {
        setQuery(response.query);
        setTableHeaders(response.tableHeaders);
      }}
      finalStep={({ table }) => (
        <WizardStep>
          {wizardStepProps => {
            if (!table || !tableHeaders) {
              return null;
            }

            return (
              <>
                <WizardStepHeading {...wizardStepProps}>
                  {dataBlock ? 'Update data block' : 'Create data block'}
                </WizardStepHeading>

                <div className="govuk-!-margin-bottom-4">
                  <TableHeadersForm
                    initialValues={tableHeaders}
                    id="dataBlockSourceWizard-tableHeadersForm"
                    onSubmit={async nextTableHeaders => {
                      setTableHeaders(nextTableHeaders);

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
                    tableHeadersConfig={tableHeaders}
                  />
                </div>

                <hr />

                <DataBlockDetailsForm
                  initialValues={{
                    heading:
                      dataBlock?.heading ??
                      generateTableTitle(table.subjectMeta),
                    name: dataBlock?.name ?? '',
                    source: dataBlock?.source ?? '',
                  }}
                  onTitleChange={setCaptionTitle}
                  onSubmit={handleSubmit}
                />
              </>
            );
          }}
        </WizardStep>
      )}
    />
  );
};

export default DataBlockSourceWizard;
