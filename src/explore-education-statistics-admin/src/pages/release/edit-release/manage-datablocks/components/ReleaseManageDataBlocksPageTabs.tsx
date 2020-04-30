import ChartBuilderTabSection from '@admin/pages/release/edit-release/manage-datablocks/components/ChartBuilderTabSection';
import DataBlockSourceWizard, {
  SavedDataBlock,
} from '@admin/pages/release/edit-release/manage-datablocks/components/DataBlockSourceWizard';
import TableTabSection from '@admin/pages/release/edit-release/manage-datablocks/components/TableTabSection';
import dataBlocksService, {
  ReleaseDataBlock,
} from '@admin/services/release/edit-release/datablocks/service';
import { DeleteDataBlockPlan } from '@admin/services/release/edit-release/datablocks/types';
import Button from '@common/components/Button';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import { TableToolState } from '@common/modules/table-tool/components/TableToolWizard';
import initialiseFromQuery from '@common/modules/table-tool/components/utils/initialiseFromQuery';
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import tableBuilderService from '@common/services/tableBuilderService';
import React, { useCallback, useEffect, useState } from 'react';

interface Props {
  releaseId: string;
  selectedDataBlock?: ReleaseDataBlock;
  onDataBlockSave: (dataBlock: ReleaseDataBlock) => void;
  onDataBlockDelete: (dataBlock: ReleaseDataBlock) => void;
}

interface DeleteDataBlock {
  plan: DeleteDataBlockPlan;
  block: ReleaseDataBlock;
}

const ReleaseManageDataBlocksPageTabs = ({
  releaseId,
  selectedDataBlock,
  onDataBlockDelete,
  onDataBlockSave,
}: Props) => {
  const [activeTab, setActiveTab] = useState<string>('');
  const [isLoading, setLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);

  const [tableToolState, setInitialTableToolState] = useState<TableToolState>();

  const [deleteDataBlock, setDeleteDataBlock] = useState<DeleteDataBlock>();

  useEffect(() => {
    if (!selectedDataBlock) {
      setLoading(false);
      return;
    }

    setLoading(true);

    const query = {
      ...selectedDataBlock.dataBlockRequest,
      includeGeoJson: selectedDataBlock.charts.some(
        chart => chart.type === 'map',
      ),
    };

    tableBuilderService.getTableData(query).then(async response => {
      const state = await initialiseFromQuery(query);
      const table = mapFullTable(response);

      setInitialTableToolState({
        ...state,
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
  }, [selectedDataBlock]);

  const handleDataBlockSave = useCallback(
    async (dataBlock: SavedDataBlock) => {
      setIsSaving(true);

      let newDataBlock: ReleaseDataBlock;

      if (dataBlock.id) {
        newDataBlock = await dataBlocksService.putDataBlock(
          dataBlock.id,
          dataBlock as ReleaseDataBlock,
        );

        setIsSaving(false);
      } else {
        newDataBlock = await dataBlocksService.postDataBlock(
          releaseId,
          dataBlock,
        );

        setIsSaving(false);
      }

      onDataBlockSave(newDataBlock);
    },
    [onDataBlockSave, releaseId],
  );

  return (
    <div style={{ position: 'relative' }}>
      {(isLoading || isSaving) && (
        <LoadingSpinner
          text={`${isSaving ? 'Saving data block' : 'Loading data block'}`}
          overlay
        />
      )}

      <div>
        <h2>
          {selectedDataBlock && selectedDataBlock
            ? selectedDataBlock.name || 'title not set'
            : 'Create new data block'}
        </h2>

        <Tabs
          openId={activeTab}
          onToggle={tab => {
            setActiveTab(tab.id);
          }}
          id="manageDataBlocks"
        >
          <TabsSection title="Data source">
            <p>Configure the data source for the data block</p>

            {selectedDataBlock && (
              <>
                <div className="govuk-!-margin-top-4">
                  <Button
                    type="button"
                    onClick={() =>
                      dataBlocksService
                        .getDeleteBlockPlan(releaseId, selectedDataBlock.id)
                        .then(plan => {
                          setDeleteDataBlock({
                            plan,
                            block: selectedDataBlock,
                          });
                        })
                    }
                  >
                    Delete this data block
                  </Button>
                </div>
              </>
            )}

            <ModalConfirm
              title="Delete data block"
              mounted={deleteDataBlock !== undefined}
              onConfirm={() => {
                if (deleteDataBlock) {
                  onDataBlockDelete(deleteDataBlock.block);
                }
              }}
              onExit={() => setDeleteDataBlock(undefined)}
              onCancel={() => setDeleteDataBlock(undefined)}
            >
              <p>Are you sure you wish to delete this data block?</p>
              <ul>
                {deleteDataBlock?.plan?.dependentDataBlocks?.map(block => (
                  <li key={block.name}>
                    <p>{block.name}</p>
                    {block.contentSectionHeading && (
                      <p>
                        {`It will be removed from the "${block.contentSectionHeading}" content section.`}
                      </p>
                    )}
                    {block.infographicFilenames.length > 0 && (
                      <p>
                        The following infographic files will also be removed:
                        <ul>
                          {block.infographicFilenames.map(filename => (
                            <li key={filename}>
                              <p>{filename}</p>
                            </li>
                          ))}
                        </ul>
                      </p>
                    )}
                  </li>
                ))}
              </ul>
            </ModalConfirm>

            {!isLoading && (
              <DataBlockSourceWizard
                releaseId={releaseId}
                dataBlock={selectedDataBlock}
                initialTableToolState={tableToolState}
                onDataBlockSave={handleDataBlockSave}
              />
            )}
          </TabsSection>

          {!isLoading &&
            selectedDataBlock &&
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
    </div>
  );
};

export default ReleaseManageDataBlocksPageTabs;
