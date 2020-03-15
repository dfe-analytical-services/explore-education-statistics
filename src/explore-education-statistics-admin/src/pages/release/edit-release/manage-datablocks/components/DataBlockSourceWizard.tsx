import DataBlockDetailsForm, {
  DataBlockDetailsFormValues,
} from '@admin/pages/release/edit-release/manage-datablocks/components/DataBlockDetailsForm';
import LoadingSpinner from '@common/components/LoadingSpinner';
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
import {
  TableDataQuery,
  TimeIdentifier,
} from '@common/modules/table-tool/services/tableBuilderService';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import getDefaultTableHeaderConfig, {
  TableHeadersConfig,
} from '@common/modules/table-tool/utils/tableHeaders';
import {
  DataBlock,
  DataBlockResponse,
  GeographicLevel,
} from '@common/services/dataBlockService';
import React, {
  createRef,
  useCallback,
  useEffect,
  useMemo,
  useState,
} from 'react';

interface CreateDataBlockProps {
  releaseId: string;
  dataBlock?: DataBlock;
  dataBlockResponse?: DataBlockResponse;
  loading?: boolean;
  onDataBlockSave: (dataBlock: DataBlock) => void;
  onTableToolLoaded?: () => void;
}

const DataBlockSourceWizard = ({
  releaseId,
  dataBlock,
  dataBlockResponse,
  onDataBlockSave,
  onTableToolLoaded,
}: CreateDataBlockProps) => {
  const dataTableRef = createRef<HTMLTableElement>();

  const [isLoading, setLoading] = useState(true);

  const [query, setQuery] = useState<TableDataQuery | undefined>(
    dataBlock?.dataBlockRequest,
  );
  const [table, setTable] = useState<FullTable | undefined>(
    dataBlockResponse && mapDataBlockResponseToFullTable(dataBlockResponse),
  );
  const [tableHeaders, setTableHeaders] = useState<TableHeadersConfig>();
  const [tableToolState, setTableToolState] = useState<TableToolState>();

  const [captionTitle, setCaptionTitle] = useState<string>(
    dataBlock?.heading ?? '',
  );

  useEffect(() => {
    const initialize = async () => {
      if (!dataBlock || !dataBlockResponse) {
        return;
      }

      if (dataBlock.dataBlockRequest) {
        await initialiseFromQuery(dataBlock.dataBlockRequest).then(state => {
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
    };

    setLoading(true);
    setTableToolState(undefined);

    initialize().then(() => {
      setLoading(false);
    });
  }, [dataBlock, dataBlockResponse, onTableToolLoaded]);

  const initialValues: DataBlockDetailsFormValues = useMemo(() => {
    if (!dataBlock) {
      return {
        heading: table ? generateTableTitle(table.subjectMeta) : '',
        name: '',
        source: '',
      };
    }

    const { heading = '', name = '', source = '' } = dataBlock;

    return {
      heading,
      name,
      source,
    };
  }, [dataBlock, table]);

  const handleSubmit = useCallback(
    (values: DataBlockDetailsFormValues) => {
      if (!query || !tableHeaders) {
        return;
      }

      const savedDataBlock: DataBlock = {
        ...values,
        id: dataBlock ? dataBlock.id : undefined,
        dataBlockRequest: {
          ...query,
          geographicLevel: query.geographicLevel as GeographicLevel,
          timePeriod: query.timePeriod && {
            ...query.timePeriod,
            startCode: query.timePeriod.startCode as TimeIdentifier,
            endCode: query.timePeriod.endCode as TimeIdentifier,
          },
        },
        tables: [],
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

  return !isLoading ? (
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
                      captionTitle={captionTitle}
                      tableHeadersConfig={tableHeaders}
                    />
                  </div>

                  <hr />

                  <DataBlockDetailsForm
                    initialValues={initialValues}
                    onTitleChange={setCaptionTitle}
                    onSubmit={handleSubmit}
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
