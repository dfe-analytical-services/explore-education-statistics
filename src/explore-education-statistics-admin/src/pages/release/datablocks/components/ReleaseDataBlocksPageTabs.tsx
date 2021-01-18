import ChartBuilderTabSection, {
  ChartBuilderTableUpdateHandler,
} from '@admin/pages/release/datablocks/components/chart/ChartBuilderTabSection';
import DataBlockSourceWizard, {
  DataBlockSourceWizardSaveHandler,
} from '@admin/pages/release/datablocks/components/DataBlockSourceWizard';
import TableTabSection from '@admin/pages/release/datablocks/components/TableTabSection';
import dataBlocksService, {
  CreateReleaseDataBlock,
  ReleaseDataBlock,
} from '@admin/services/dataBlockService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import filterOrphanedDataSets from '@common/modules/charts/util/filterOrphanedDataSets';
import { InitialTableToolState } from '@common/modules/table-tool/components/TableToolWizard';
import getInitialStepSubjectMeta from '@common/modules/table-tool/components/utils/getInitialStepSubjectMeta';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import mapUnmappedTableHeaders from '@common/modules/table-tool/utils/mapUnmappedTableHeaders';
import logger from '@common/services/logger';
import tableBuilderService, {
  ReleaseTableDataQuery,
} from '@common/services/tableBuilderService';
import minDelay from '@common/utils/minDelay';
import produce from 'immer';
import omit from 'lodash/omit';
import React, { useCallback, useState } from 'react';

export type SavedDataBlock = CreateReleaseDataBlock & {
  id?: string;
};

interface Props {
  releaseId: string;
  selectedDataBlock?: ReleaseDataBlock;
  onDataBlockSave: (dataBlock: ReleaseDataBlock) => void;
}

const ReleaseDataBlocksPageTabs = ({
  releaseId,
  selectedDataBlock,
  onDataBlockSave,
}: Props) => {
  // Track number of saves as we can use this to
  // force re-rendering of the tab sections.
  const [saveNumber, setSaveNumber] = useState(0);
  const [isSaving, setIsSaving] = useState(false);

  const {
    value: tableState,
    setState: setTableState,
    error,
    isLoading,
  } = useAsyncRetry<InitialTableToolState>(async () => {
    const { subjects } = await tableBuilderService.getReleaseMeta(releaseId);

    if (!selectedDataBlock) {
      return {
        initialStep: 1,
        subjects,
        query: {
          releaseId,
          subjectId: '',
          includeGeoJson: false,
          locations: {},
          filters: [],
          indicators: [],
        },
      };
    }

    const query: ReleaseTableDataQuery = {
      ...selectedDataBlock.query,
      releaseId,
      includeGeoJson: selectedDataBlock.charts.some(
        chart => chart.type === 'map',
      ),
    };

    const tableData = await tableBuilderService.getTableData(query);
    const { initialStep, subjectMeta } = await getInitialStepSubjectMeta(
      query,
      tableData,
    );

    // Reduce step by 1 as there is no publication step
    const step = initialStep - 1;

    if (step < 5) {
      return {
        initialStep: step,
        subjects,
        subjectMeta,
        query,
      };
    }

    const table = mapFullTable(tableData);

    try {
      const tableHeaders = mapTableHeadersConfig(
        selectedDataBlock.table.tableHeaders,
        table.subjectMeta,
      );

      return {
        initialStep: step,
        subjects,
        subjectMeta,
        query,
        response: {
          table,
          tableHeaders,
        },
      };
    } catch (err) {
      logger.error(err);

      // Return to step 2 if anything is wrong
      // with producing the table headers.
      return {
        initialStep: 2,
        subjectMeta,
        query,
      };
    }
  }, []);

  const handleDataBlockSave = useCallback(
    async (dataBlock: SavedDataBlock) => {
      setIsSaving(true);

      const dataBlockToSave: SavedDataBlock = {
        ...dataBlock,
        query: {
          ...(omit(dataBlock.query, ['releaseId']) as SavedDataBlock['query']),
          includeGeoJson: dataBlock.charts[0]?.type === 'map',
        },
      };

      const newDataBlock = await minDelay(() => {
        if (dataBlockToSave.id) {
          return dataBlocksService.updateDataBlock(
            dataBlockToSave.id,
            dataBlockToSave as ReleaseDataBlock,
          );
        }

        return dataBlocksService.createDataBlock(releaseId, dataBlockToSave);
      }, 500);

      onDataBlockSave(newDataBlock);

      setIsSaving(false);
      setSaveNumber(saveNumber + 1);
    },
    [onDataBlockSave, releaseId, saveNumber],
  );

  const handleDataBlockSourceSave: DataBlockSourceWizardSaveHandler = useCallback(
    async ({ query, table, tableHeaders, details }) => {
      const charts = produce(selectedDataBlock?.charts ?? [], draft => {
        const majorAxis = draft[0]?.axes?.major;

        if (majorAxis?.dataSets) {
          majorAxis.dataSets = filterOrphanedDataSets(
            majorAxis.dataSets,
            table.subjectMeta,
          );
        }
      });

      setTableState({
        value: {
          initialStep: 5,
          query,
          response: {
            table,
            tableHeaders,
          },
        },
      });

      await handleDataBlockSave({
        ...(selectedDataBlock ?? {}),
        ...details,
        query,
        charts,
        table: {
          tableHeaders: mapUnmappedTableHeaders(tableHeaders),
          indicators: [],
        },
      });
    },
    [handleDataBlockSave, selectedDataBlock, setTableState],
  );

  const handleTableHeadersSave = useCallback(
    async (tableHeaders: TableHeadersConfig) => {
      if (!selectedDataBlock) {
        throw new Error(
          'Cannot save table headers when no data block has been selected',
        );
      }

      if (!tableState?.response) {
        throw new Error(
          'Cannot save table headers when table has not been created',
        );
      }

      setTableState({
        value: {
          ...tableState,
          response: {
            ...tableState?.response,
            tableHeaders,
          },
        },
      });

      await handleDataBlockSave({
        ...selectedDataBlock,
        table: {
          tableHeaders: mapUnmappedTableHeaders(tableHeaders),
          indicators: [],
        },
      });
    },
    [handleDataBlockSave, selectedDataBlock, setTableState, tableState],
  );

  const handleChartTableUpdate: ChartBuilderTableUpdateHandler = useCallback(
    ({ table, query }) => {
      if (!tableState?.response) {
        throw new Error('Cannot update uninitialised table state');
      }

      setTableState({
        value: {
          ...tableState,
          query,
          response: {
            ...tableState?.response,
            table,
          },
        },
      });
    },
    [setTableState, tableState],
  );

  const { query, response } = tableState ?? {};

  return (
    <div style={{ position: 'relative' }} className="govuk-!-padding-top-2">
      {(isLoading || isSaving) && (
        <LoadingSpinner
          text={`${isSaving ? 'Saving data block' : 'Loading data block'}`}
          overlay
        />
      )}

      {selectedDataBlock && tableState && tableState?.initialStep < 5 && (
        <WarningMessage>
          There is a problem with this data block as we could not render a table
          with the selected options. Please re-check your choices to ensure the
          correct table can be produced.
        </WarningMessage>
      )}

      {!error ? (
        <Tabs id="manageDataBlocks">
          <TabsSection title="Data source" id="manageDataBlocks-dataSource">
            {!isLoading && tableState && (
              <DataBlockSourceWizard
                key={saveNumber}
                dataBlock={selectedDataBlock}
                tableToolState={tableState}
                onSave={handleDataBlockSourceSave}
              />
            )}
          </TabsSection>

          {selectedDataBlock &&
            query &&
            response && [
              <TabsSection
                title="Table"
                key="table"
                id="manageDataBlocks-table"
                lazy
              >
                <TableTabSection
                  table={response.table}
                  tableHeaders={response.tableHeaders}
                  onSave={handleTableHeadersSave}
                />
              </TabsSection>,
              <TabsSection
                title="Chart"
                key="chart"
                id="manageDataBlocks-chart"
              >
                <ChartBuilderTabSection
                  key={saveNumber}
                  dataBlock={selectedDataBlock}
                  query={query}
                  releaseId={releaseId}
                  table={response.table}
                  onDataBlockSave={handleDataBlockSave}
                  onTableUpdate={handleChartTableUpdate}
                />
              </TabsSection>,
            ]}
        </Tabs>
      ) : (
        <WarningMessage>
          There was a problem loading data block source
        </WarningMessage>
      )}
    </div>
  );
};

export default ReleaseDataBlocksPageTabs;
