import DataBlockDetailsForm, {
  DataBlockDetailsFormValues,
} from '@admin/pages/release/edit-release/manage-datablocks/components/DataBlockDetailsForm';
import { mapDataBlockResponseToFullTable } from '@common/modules/find-statistics/components/util/tableUtil';
import { generateTableTitle } from '@common/modules/table-tool/components/DataTableCaption';
import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import TableToolWizard, {
  TableToolState,
} from '@common/modules/table-tool/components/TableToolWizard';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import initialiseFromQuery from '@common/modules/table-tool/components/utils/initialiseFromQuery';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import { TableDataQuery } from '@common/modules/table-tool/services/tableBuilderService';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import getDefaultTableHeaderConfig, {
  TableHeadersConfig,
} from '@common/modules/table-tool/utils/tableHeaders';
import {
  DataBlock,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import React, { createRef, useEffect, useState } from 'react';

interface CreateDataBlockProps {
  releaseId: string;
  dataBlock?: DataBlock;
  dataBlockResponse?: DataBlockResponse;
  loading?: boolean;
  onDataBlockSave: (
    dataBlock: DataBlock,
    newDataBlockResponse?: TableToolState,
  ) => Promise<DataBlock>;
  onTableToolLoaded?: () => void;
}

const DataBlockSourceWizard = ({
  releaseId,
  dataBlock,
  dataBlockResponse,
  loading = false,
  onDataBlockSave,
  onTableToolLoaded,
}: CreateDataBlockProps) => {
  const dataTableRef = createRef<HTMLTableElement>();

  const [query, setQuery] = useState<TableDataQuery | undefined>(
    dataBlock?.dataBlockRequest,
  );
  const [table, setTable] = useState<FullTable | undefined>(
    dataBlockResponse && mapDataBlockResponseToFullTable(dataBlockResponse),
  );
  const [tableHeaders, setTableHeaders] = useState<TableHeadersConfig>();
  const [tableToolState, setTableToolState] = useState<TableToolState>();

  const [initialValues, setInitialValues] = useState<
    DataBlockDetailsFormValues
  >();

  useEffect(() => {
    if (dataBlock && dataBlockResponse) {
      if (dataBlock.dataBlockRequest) {
        initialiseFromQuery(dataBlock.dataBlockRequest).then(state => {
          setTableToolState(state);

          if (onTableToolLoaded) {
            onTableToolLoaded();
          }
        });
      }

      setQuery(dataBlock.dataBlockRequest);

      const dataTable = mapDataBlockResponseToFullTable(dataBlockResponse);
      setTable(dataTable);

      if (dataBlock?.tables?.length) {
        setTableHeaders(
          mapTableHeadersConfig(
            dataBlock?.tables?.[0]?.tableHeaders,
            dataTable.subjectMeta,
          ),
        );
      } else {
        setTableHeaders(getDefaultTableHeaderConfig(dataTable.subjectMeta));
      }
    }
  }, [dataBlock, dataBlockResponse, onTableToolLoaded]);

  useEffect(() => {
    if (!dataBlock) {
      setInitialValues({
        title: table ? generateTableTitle(table.subjectMeta) : '',
        name: '',
        source: '',
      });
      return;
    }

    const { heading: title = '', name = '', source = '' } = dataBlock;

    setInitialValues({
      title,
      name,
      source,
    });
  }, [dataBlock, table]);

  return !loading ? (
    <TableToolWizard
      releaseId={releaseId}
      themeMeta={[]}
      initialState={tableToolState}
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
                {initialValues ? 'Update data block' : 'Create data block'}
              </WizardStepHeading>

              {query && tableHeaders && table && (
                <>
                  <div className="govuk-!-margin-bottom-4">
                    <TableHeadersForm
                      initialValues={tableHeaders}
                      id="dataBlockSourceWizard-tableHeadersForm"
                      onSubmit={async nextTableHeaders => {
                        setTableHeaders(nextTableHeaders);

                        if (dataBlock) {
                          await onDataBlockSave({
                            ...dataBlock,
                            tables: [
                              {
                                tableHeaders: nextTableHeaders,
                                indicators: [],
                              },
                            ],
                          });
                        }

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
                      tableHeadersConfig={tableHeaders}
                    />
                  </div>

                  <hr />

                  <DataBlockDetailsForm
                    initialValues={initialValues}
                    query={query}
                    tableHeaders={tableHeaders}
                    initialDataBlock={dataBlock}
                    releaseId={releaseId}
                    onDataBlockSave={data =>
                      onDataBlockSave({
                        ...data,
                        tables: [
                          {
                            tableHeaders,
                            indicators: [],
                          },
                        ],
                      })
                    }
                  />
                </>
              )}
            </>
          )}
        </WizardStep>
      )}
    />
  ) : null;
};

export default DataBlockSourceWizard;
