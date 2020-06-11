import ChartBuilderTabSection from '@admin/pages/release/edit-release/manage-datablocks/components/ChartBuilderTabSection';
import DataBlockSourceWizard, {
  DataBlockSourceWizardSaveHandler,
} from '@admin/pages/release/edit-release/manage-datablocks/components/DataBlockSourceWizard';
import TableTabSection from '@admin/pages/release/edit-release/manage-datablocks/components/TableTabSection';
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
  TableDataQuery,
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
  query: TableDataQuery;
}

interface Props {
  releaseId: string;
  selectedDataBlock?: ReleaseDataBlock;
  onDataBlockSave: (dataBlock: ReleaseDataBlock) => void;
}

const ReleaseManageDataBlocksPageTabs = ({
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
    setValue: setTableState,
    error,
    isLoading,
  } = useAsyncRetry<TableState | undefined>(async () => {
    if (!selectedDataBlock) {
      return undefined;
    }

    const query = {
      ...selectedDataBlock.dataBlockRequest,
      includeGeoJson: selectedDataBlock?.chart?.type === 'map',
    };

    const tableData = await tableBuilderService.getTableData(query);
    const nextSubjectMeta = await tableBuilderService.getPublicationSubjectMeta(
      query.subjectId,
    );

    const table = mapFullTable(tableData);

    setSubjectMeta(nextSubjectMeta);

    const tableHeaders = selectedDataBlock
      ? mapTableHeadersConfig(
          selectedDataBlock.tables[0].tableHeaders,
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
        ...tableState,
        ...nextTableState,
      });
    },
    [setTableState, tableState],
  );

  const handleDataBlockSave = useCallback(
    async (dataBlock: SavedDataBlock) => {
      setIsSaving(true);

      const dataBlockToSave: SavedDataBlock = {
        ...dataBlock,
        dataBlockRequest: {
          ...dataBlock.dataBlockRequest,
          includeGeoJson: dataBlock.chart?.type === 'map',
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
      const chart = produce(selectedDataBlock?.chart, draft => {
        const majorAxis = draft?.axes?.major;

        if (majorAxis?.dataSets) {
          majorAxis.dataSets = filterOrphanedDataSets(
            majorAxis.dataSets,
            table.subjectMeta,
          );
        }
      });

      setTableState({
        query,
        table,
        tableHeaders,
      });

      await handleDataBlockSave({
        ...(selectedDataBlock ?? {}),
        ...details,
        dataBlockRequest: query,
        chart,
        tables: [
          {
            tableHeaders: mapUnmappedTableHeaders(tableHeaders),
            indicators: [],
          },
        ],
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
        tables: [
          {
            tableHeaders: mapUnmappedTableHeaders(tableHeaders),
            indicators: [],
          },
        ],
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
                releaseId={releaseId}
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

export default ReleaseManageDataBlocksPageTabs;
