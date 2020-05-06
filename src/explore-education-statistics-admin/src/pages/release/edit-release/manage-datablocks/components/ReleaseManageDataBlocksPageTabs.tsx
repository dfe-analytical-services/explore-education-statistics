import ChartBuilderTabSection from '@admin/pages/release/edit-release/manage-datablocks/components/ChartBuilderTabSection';
import DataBlockSourceWizard, {
  SavedDataBlock,
} from '@admin/pages/release/edit-release/manage-datablocks/components/DataBlockSourceWizard';
import TableTabSection from '@admin/pages/release/edit-release/manage-datablocks/components/TableTabSection';
import dataBlocksService, {
  ReleaseDataBlock,
} from '@admin/services/release/edit-release/datablocks/service';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import WarningMessage from '@common/components/WarningMessage';
import useTableQuery from '@common/modules/find-statistics/hooks/useTableQuery';
import { TableToolState } from '@common/modules/table-tool/components/TableToolWizard';
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import tableBuilderService, {
  TableDataQuery,
} from '@common/services/tableBuilderService';
import minDelay from '@common/utils/minDelay';
import React, { useCallback, useMemo, useState } from 'react';

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
  const [isSaving, setIsSaving] = useState(false);

  // Track number of saves as we can use this to
  // force re-rendering of the tab sections.
  const [saveNumber, setSaveNumber] = useState(0);

  const [tableToolState, setTableToolState] = useState<TableToolState>();

  const initialQuery = useMemo<TableDataQuery | undefined>(
    () =>
      selectedDataBlock
        ? {
            ...selectedDataBlock.dataBlockRequest,
            includeGeoJson: selectedDataBlock.charts.some(
              chart => chart.type === 'map',
            ),
          }
        : undefined,
    [selectedDataBlock],
  );

  const { error, isLoading } = useTableQuery(initialQuery, {
    onSuccess: async (table, query) => {
      const subjectMeta = await tableBuilderService.filterPublicationSubjectMeta(
        query,
      );

      setTableToolState({
        initialStep: 5,
        query,
        subjectMeta,
        response: {
          table,
          tableHeaders: selectedDataBlock
            ? mapTableHeadersConfig(
                selectedDataBlock.tables[0].tableHeaders,
                table.subjectMeta,
              )
            : getDefaultTableHeaderConfig(table.subjectMeta),
        },
      });
    },
  });

  const handleDataBlockSave = useCallback(
    async (dataBlock: SavedDataBlock) => {
      setIsSaving(true);

      const newDataBlock = await minDelay(() => {
        if (dataBlock.id) {
          return dataBlocksService.putDataBlock(
            dataBlock.id,
            dataBlock as ReleaseDataBlock,
          );
        }

        return dataBlocksService.postDataBlock(releaseId, dataBlock);
      }, 500);

      onDataBlockSave(newDataBlock);

      setIsSaving(false);
      setSaveNumber(saveNumber + 1);
    },
    [onDataBlockSave, releaseId, saveNumber],
  );

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
                releaseId={releaseId}
                dataBlock={selectedDataBlock}
                initialTableToolState={tableToolState}
                onDataBlockSave={handleDataBlockSave}
              />
            )}
          </TabsSection>

          {selectedDataBlock && [
            <TabsSection title="Table" key="table" id="manageDataBlocks-table">
              {tableToolState?.response && (
                <TableTabSection
                  dataBlock={selectedDataBlock}
                  table={tableToolState.response.table}
                  tableHeaders={tableToolState.response.tableHeaders}
                  onDataBlockSave={handleDataBlockSave}
                />
              )}
            </TabsSection>,
            <TabsSection title="Chart" key="chart" id="manageDataBlocks-chart">
              {tableToolState?.response && initialQuery && (
                <ChartBuilderTabSection
                  key={saveNumber}
                  dataBlock={selectedDataBlock}
                  query={initialQuery}
                  table={tableToolState.response.table}
                  onDataBlockSave={handleDataBlockSave}
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
