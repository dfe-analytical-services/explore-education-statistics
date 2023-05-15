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
import isOrphanedDataSet from '@common/modules/charts/util/isOrphanedDataSet';
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
  dataBlock?: ReleaseDataBlock;
  onDataBlockSave?: (dataBlock: ReleaseDataBlock) => void;
}

const DataBlockPageTabs = ({
  releaseId,
  dataBlock,
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
    const subjects = await tableBuilderService.listReleaseSubjects(releaseId);

    if (!dataBlock) {
      return {
        initialStep: 1,
        subjects,
        query: {
          releaseId,
          subjectId: '',
          includeGeoJson: false,
          locationIds: [],
          filters: [],
          indicators: [],
        },
      };
    }

    const query: ReleaseTableDataQuery = {
      ...dataBlock.query,
      releaseId,
      includeGeoJson: dataBlock.charts.some(chart => chart.type === 'map'),
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
        dataBlock.table.tableHeaders,
        table,
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
    async (nextDataBlock: SavedDataBlock) => {
      setIsSaving(true);

      const dataBlockToSave: SavedDataBlock = {
        ...nextDataBlock,
        query: {
          ...(omit(nextDataBlock.query, [
            'releaseId',
          ]) as SavedDataBlock['query']),
          includeGeoJson: nextDataBlock.charts[0]?.type === 'map',
        },
      };

      const savedDataBlock = await minDelay(() => {
        if (dataBlockToSave.id) {
          return dataBlocksService.updateDataBlock(
            dataBlockToSave.id,
            dataBlockToSave as ReleaseDataBlock,
          );
        }

        return dataBlocksService.createDataBlock(releaseId, dataBlockToSave);
      }, 500);

      if (onDataBlockSave) {
        onDataBlockSave(savedDataBlock);
      }

      setIsSaving(false);
      setSaveNumber(saveNumber + 1);
    },
    [onDataBlockSave, releaseId, saveNumber],
  );

  const handleDataBlockSourceSave: DataBlockSourceWizardSaveHandler = useCallback(
    async ({ query, table, tableHeaders, details }) => {
      const charts = produce(dataBlock?.charts ?? [], draft => {
        const majorAxis = draft[0]?.axes?.major;
        const legend = draft[0]?.legend;

        // If old chart title is the same as the old table title, then the chart title is defaulting to
        // the table title, so don't inadvertently set the old chart/table title
        if (
          dataBlock?.charts[0] &&
          dataBlock?.charts[0]?.title === dataBlock?.heading
        ) {
          draft[0].title = undefined;
        }

        // Remove data sets that are no longer applicable to a given table's subject meta.
        if (majorAxis?.dataSets) {
          majorAxis.dataSets = majorAxis.dataSets.filter(
            dataSet => !isOrphanedDataSet(dataSet, table.subjectMeta),
          );
        }
        if (legend?.items) {
          legend.items = legend.items.filter(
            item => !isOrphanedDataSet(item.dataSet, table.subjectMeta),
          );
        }
      });

      await handleDataBlockSave({
        ...(dataBlock ?? {}),
        ...details,
        query,
        charts,
        table: {
          tableHeaders: mapUnmappedTableHeaders(tableHeaders),
          indicators: [],
        },
      });

      setTableState({
        value: {
          ...tableState,
          initialStep: 5,
          query,
          response: {
            table,
            tableHeaders,
          },
        },
      });
    },
    [handleDataBlockSave, dataBlock, setTableState, tableState],
  );

  const handleTableHeadersSave = useCallback(
    async (tableHeaders: TableHeadersConfig) => {
      if (!dataBlock) {
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
        ...dataBlock,
        table: {
          tableHeaders: mapUnmappedTableHeaders(tableHeaders),
          indicators: [],
        },
      });
    },
    [handleDataBlockSave, dataBlock, setTableState, tableState],
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

      {dataBlock && tableState && tableState?.initialStep < 5 && (
        <WarningMessage>
          There is a problem with this data block as we could not render a table
          with the selected options. Please re-check your choices to ensure the
          correct table can be produced.
        </WarningMessage>
      )}

      {!error ? (
        <Tabs id="dataBlockTabs">
          <TabsSection title="Data source" id="dataBlockTabs-dataSource">
            {!isLoading && tableState && (
              <DataBlockSourceWizard
                key={saveNumber}
                dataBlock={dataBlock}
                tableToolState={tableState}
                onSave={handleDataBlockSourceSave}
              />
            )}
          </TabsSection>

          {dataBlock &&
            query &&
            response && [
              <TabsSection
                title="Table"
                key="table"
                id="dataBlockTabs-table"
                lazy
              >
                <TableTabSection
                  dataBlock={dataBlock}
                  table={response.table}
                  tableHeaders={response.tableHeaders}
                  onReorderTableHeaders={handleTableHeadersSave}
                />
              </TabsSection>,
              <TabsSection
                title="Chart"
                key="chart"
                id="dataBlockTabs-chart"
                lazy
              >
                <ChartBuilderTabSection
                  key={saveNumber}
                  dataBlock={dataBlock}
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

export default DataBlockPageTabs;
