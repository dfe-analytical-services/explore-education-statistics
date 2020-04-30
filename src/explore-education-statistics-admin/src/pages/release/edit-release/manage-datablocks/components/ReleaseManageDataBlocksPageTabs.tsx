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
import useTableQuery from '@common/modules/find-statistics/hooks/useTableQuery';
import { TableToolState } from '@common/modules/table-tool/components/TableToolWizard';
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import tableBuilderService from '@common/services/tableBuilderService';
import minDelay from '@common/utils/minDelay';
import React, { useCallback, useEffect, useMemo, useState } from 'react';

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
  const [activeTab, setActiveTab] = useState<string>('');

  const [isLoading, setLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);

  const [tableToolState, setInitialTableToolState] = useState<TableToolState>();

  const query = useMemo(
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

  const { value: table } = useTableQuery(query);

  useEffect(() => {
    if (!query) {
      setLoading(false);
      return;
    }

    setLoading(true);

    if (!table) {
      return;
    }

    tableBuilderService
      .filterPublicationSubjectMeta(query)
      .then(subjectMeta => {
        setInitialTableToolState({
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

        setLoading(false);
      });
  }, [query, selectedDataBlock, table]);

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
    },
    [onDataBlockSave, releaseId],
  );

  return (
    <div style={{ position: 'relative' }} className="govuk-!-padding-top-2">
      {(isLoading || isSaving) && (
        <LoadingSpinner
          text={`${isSaving ? 'Saving data block' : 'Loading data block'}`}
          overlay
        />
      )}

      <Tabs
        id="manageDataBlocks"
        openId={activeTab}
        onToggle={tab => {
          setActiveTab(tab.id);
        }}
      >
        <TabsSection title="Data source">
          <p>Configure the data source for the data block</p>

          {!isLoading && (
            <DataBlockSourceWizard
              releaseId={releaseId}
              dataBlock={selectedDataBlock}
              initialTableToolState={tableToolState}
              onDataBlockSave={handleDataBlockSave}
            />
          )}
        </TabsSection>

        {selectedDataBlock &&
          tableToolState?.response && [
            <TabsSection title="Table" key="table">
              <TableTabSection
                dataBlock={selectedDataBlock}
                table={tableToolState.response.table}
                tableHeaders={tableToolState.response.tableHeaders}
                onDataBlockSave={handleDataBlockSave}
              />
            </TabsSection>,
            <TabsSection title="Chart" key="chart">
              <ChartBuilderTabSection
                dataBlock={selectedDataBlock}
                table={tableToolState.response.table}
                onDataBlockSave={handleDataBlockSave}
              />
            </TabsSection>,
          ]}
      </Tabs>
    </div>
  );
};

export default ReleaseManageDataBlocksPageTabs;
