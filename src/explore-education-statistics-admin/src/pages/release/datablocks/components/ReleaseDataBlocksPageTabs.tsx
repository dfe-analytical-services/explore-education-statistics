import ChartBuilderTabSection from '@admin/pages/release/datablocks/components/ChartBuilderTabSection';
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
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import mapUnmappedTableHeaders from '@common/modules/table-tool/utils/mapUnmappedTableHeaders';
import tableBuilderService, {
  PublicationSubjectMeta,
  ReleaseTableDataQuery,
} from '@common/services/tableBuilderService';
import minDelay from '@common/utils/minDelay';
import produce from 'immer';
import React, { useCallback, useState } from 'react';

export type SavedDataBlock = CreateReleaseDataBlock & {
  id?: string;
};

interface TableState {
  table: FullTable;
  tableHeaders: TableHeadersConfig;
  query: ReleaseTableDataQuery;
}

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

  const [subjectMeta, setSubjectMeta] = useState<PublicationSubjectMeta>();

  const {
    value: tableState,
    setState: setTableState,
    error,
    isLoading,
  } = useAsyncRetry<TableState | undefined>(async () => {
    if (!selectedDataBlock) {
      return undefined;
    }

    const query: ReleaseTableDataQuery = {
      ...selectedDataBlock.query,
      releaseId,
      includeGeoJson: selectedDataBlock.charts.some(
        chart => chart.type === 'map',
      ),
    };

    const tableData = await tableBuilderService.getTableData(query);
    const nextSubjectMeta = await tableBuilderService.getPublicationSubjectMeta(
      query.subjectId,
    );

    const table = mapFullTable(tableData);

    setSubjectMeta(nextSubjectMeta);

    const tableHeaders = selectedDataBlock
      ? mapTableHeadersConfig(
          selectedDataBlock.table.tableHeaders,
          table.subjectMeta,
        )
      : getDefaultTableHeaderConfig(table.subjectMeta);

    return {
      table,
      tableHeaders,
      query,
    };
  }, []);

  const updateTableState = useCallback(
    (nextTableState: Partial<TableState>) => {
      if (!tableState) {
        throw new Error('Cannot update undefined table state');
      }

      setTableState({
        isLoading: false,
        value: {
          ...tableState,
          ...nextTableState,
        },
      });
    },
    [setTableState, tableState],
  );

  const handleDataBlockSave = useCallback(
    async (dataBlock: SavedDataBlock) => {
      setIsSaving(true);

      const dataBlockToSave: SavedDataBlock = {
        ...dataBlock,
        query: {
          ...dataBlock.query,
          includeGeoJson: dataBlock.charts[0]?.type === 'map',
        },
      };

      const newDataBlock = await minDelay(() => {
        if (dataBlockToSave.id) {
          return dataBlocksService.putDataBlock(
            dataBlockToSave.id,
            dataBlockToSave as ReleaseDataBlock,
          );
        }

        return dataBlocksService.postDataBlock(releaseId, dataBlockToSave);
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
        isLoading: false,
        value: {
          query,
          table,
          tableHeaders,
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

      updateTableState({ tableHeaders });

      await handleDataBlockSave({
        ...selectedDataBlock,
        table: {
          tableHeaders: mapUnmappedTableHeaders(tableHeaders),
          indicators: [],
        },
      });
    },
    [handleDataBlockSave, selectedDataBlock, updateTableState],
  );

  const { query, table, tableHeaders } = tableState ?? {};

  return (
    <div style={{ position: 'relative' }} className="govuk-!-padding-top-2">
      {(isLoading || isSaving) && (
        <LoadingSpinner
          text={`${isSaving ? 'Saving data block' : 'Loading data block'}`}
          overlay
        />
      )}

      {!error ? (
        <Tabs id="manageDataBlocks">
          <TabsSection title="Data source" id="manageDataBlocks-dataSource">
            {!isLoading && (
              <DataBlockSourceWizard
                key={saveNumber}
                dataBlock={selectedDataBlock}
                query={query}
                subjectMeta={subjectMeta}
                table={table}
                tableHeaders={tableHeaders}
                onSave={handleDataBlockSourceSave}
              />
            )}
          </TabsSection>

          {selectedDataBlock && [
            <TabsSection title="Table" key="table" id="manageDataBlocks-table">
              {table && tableHeaders && (
                <TableTabSection
                  table={table}
                  tableHeaders={tableHeaders}
                  onSave={handleTableHeadersSave}
                />
              )}
            </TabsSection>,
            <TabsSection title="Chart" key="chart" id="manageDataBlocks-chart">
              {query && table && (
                <ChartBuilderTabSection
                  key={saveNumber}
                  dataBlock={selectedDataBlock}
                  query={query}
                  table={table}
                  onDataBlockSave={handleDataBlockSave}
                  onTableUpdate={updateTableState}
                />
              )}
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
