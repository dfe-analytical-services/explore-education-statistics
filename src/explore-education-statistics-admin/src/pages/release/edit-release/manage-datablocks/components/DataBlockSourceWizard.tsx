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
import initialiseFromQuery from '@common/modules/table-tool/components/utils/initialiseFromQuery';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import {
  TableDataQuery,
  TableDataResponse,
} from '@common/services/tableBuilderService';
import React, {
  createRef,
  useCallback,
  useEffect,
  useMemo,
  useState,
} from 'react';

export type SavedDataBlock = CreateReleaseDataBlock & {
  id?: string;
};

interface DataBlockSourceWizardProps {
  releaseId: string;
  dataBlock?: ReleaseDataBlock;
  dataBlockResponse?: TableDataResponse;
  loading?: boolean;
  onDataBlockSave: (dataBlock: SavedDataBlock) => void;
  onTableToolLoaded?: () => void;
}

const DataBlockSourceWizard = ({
  releaseId,
  dataBlock,
  dataBlockResponse,
  onDataBlockSave,
  onTableToolLoaded,
}: DataBlockSourceWizardProps) => {
  const dataTableRef = createRef<HTMLTableElement>();

  const [isLoading, setLoading] = useState(true);

  const [query, setQuery] = useState<TableDataQuery | undefined>(
    dataBlock?.dataBlockRequest,
  );
  const [table, setTable] = useState<FullTable | undefined>(
    dataBlockResponse && mapFullTable(dataBlockResponse),
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

      const dataTable = mapFullTable(dataBlockResponse);
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
