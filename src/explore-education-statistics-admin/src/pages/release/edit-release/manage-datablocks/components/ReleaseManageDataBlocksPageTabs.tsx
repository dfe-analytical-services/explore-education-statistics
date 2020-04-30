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
import useTableQuery from '@common/modules/find-statistics/hooks/useTableQuery';
import { TableToolState } from '@common/modules/table-tool/components/TableToolWizard';
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import tableBuilderService from '@common/services/tableBuilderService';
import React, { useCallback, useEffect, useMemo, useState } from 'react';

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

      let newDataBlock: ReleaseDataBlock;

      if (dataBlock.id) {
        newDataBlock = await dataBlocksService.putDataBlock(
          dataBlock.id,
          dataBlock as ReleaseDataBlock,
        );
      } else {
        newDataBlock = await dataBlocksService.postDataBlock(
          releaseId,
          dataBlock,
        );
      }

      onDataBlockSave(newDataBlock);
      setIsSaving(false);
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
          id="manageDataBlocks"
          openId={activeTab}
          onToggle={tab => {
            setActiveTab(tab.id);
          }}
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
    </div>
  );
};

export default ReleaseManageDataBlocksPageTabs;
