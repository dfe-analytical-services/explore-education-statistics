import DataBlockContentTabs from '@admin/pages/release/edit-release/manage-datablocks/components/DataBlockContentTabs';
import DataBlockSourceWizard from '@admin/pages/release/edit-release/manage-datablocks/components/DataBlockSourceWizard';
import { manageDataBlocksRoute } from '@admin/routes/edit-release/routes';
import dataBlocksService from '@admin/services/release/edit-release/datablocks/service';
import Button from '@common/components/Button';
import { FormSelect } from '@common/components/form';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import dataBlockService, {
  DataBlock,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import React, { useCallback, useMemo, useState } from 'react';
import { RouteComponentProps } from 'react-router';

export interface ReleaseManageDataBlocksPageParams {
  publicationId: string;
  releaseId: string;
  dataBlockId?: string;
}

interface DataBlockData {
  dataBlock: DataBlock;
  response: DataBlockResponse;
}

const emptyDataBlocks: DataBlock[] = [];

const ReleaseManageDataBlocksPage = ({
  match,
  history,
}: RouteComponentProps<ReleaseManageDataBlocksPageParams>) => {
  const { publicationId, releaseId, dataBlockId } = match.params;

  const [activeTab, setActiveTab] = useState<string>('');

  const [isLoading, setLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);

  const [deleteDataBlock, setDeleteDataBlock] = useState<DataBlock>();

  const {
    value: dataBlocks = emptyDataBlocks,
    isLoading: isDataBlocksLoading,
    retry: fetchDataBlocks,
    setValue: setDataBlocks,
  } = useAsyncRetry(() => dataBlocksService.getDataBlocks(releaseId), [
    releaseId,
  ]);

  const dataBlockOptions = useMemo(
    () =>
      dataBlocks.map(({ name, id }, index) => ({
        label: `${name || index}`,
        value: `${id}`,
      })),
    [dataBlocks],
  );

  const { value: dataBlockData } = useAsyncRetry<
    DataBlockData | undefined
  >(async () => {
    if (!dataBlockId) {
      setLoading(false);

      return undefined;
    }

    if (isDataBlocksLoading) {
      return undefined;
    }

    const dataBlock = dataBlocks.find(({ id }) => dataBlockId === id);

    if (dataBlock === undefined) {
      history.push(
        manageDataBlocksRoute.generateLink({ publicationId, releaseId }),
      );

      return undefined;
    }

    const request = dataBlock.dataBlockRequest;

    if (request === undefined) {
      return undefined;
    }

    setLoading(true);

    const response = await dataBlockService.getDataBlockForSubject(request);

    if (response === undefined) {
      return undefined;
    }

    return {
      dataBlock,
      response: {
        ...response,
        releaseId,
      },
    };
  }, [dataBlockId, dataBlocks, isDataBlocksLoading]);

  const onDeleteDataBlock = useCallback(
    (dataBlock: DataBlock) => {
      setDeleteDataBlock(undefined);

      if (dataBlock.id) {
        dataBlocksService
          .deleteDataBlock(dataBlock.id)
          .then(fetchDataBlocks)
          .then(() =>
            history.push(
              manageDataBlocksRoute.generateLink({ publicationId, releaseId }),
            ),
          );
      }
    },
    [fetchDataBlocks, history, publicationId, releaseId],
  );

  const onDataBlockSave = useMemo(
    () => async (dataBlock: DataBlock) => {
      setIsSaving(true);

      let newDataBlock: DataBlock;
      let newDataBlocks: DataBlock[];

      if (dataBlock.id) {
        newDataBlock = await dataBlocksService.putDataBlock(
          dataBlock.id,
          dataBlock,
        );

        newDataBlocks = [...dataBlocks];

        const currentBlockIndex = newDataBlocks.findIndex(
          db => db.id === newDataBlock.id,
        );

        if (currentBlockIndex > -1) {
          newDataBlocks[currentBlockIndex] = newDataBlock;
        }

        setDataBlocks(newDataBlocks);
        setIsSaving(false);
      } else {
        newDataBlock = await dataBlocksService.postDataBlock(
          releaseId,
          dataBlock,
        );

        // Redirect as it's just easier to refresh the page state.
        history.push(
          manageDataBlocksRoute.generateLink({
            publicationId,
            releaseId,
            dataBlockId: newDataBlock.id,
          }),
        );
      }
    },
    [dataBlocks, setDataBlocks, releaseId, history, publicationId],
  );

  const handleTableToolLoaded = useCallback(() => setLoading(false), []);

  return (
    <>
      {dataBlockOptions.length > 0 && (
        <FormSelect
          id="selectDataBlock"
          name="selectDataBlock"
          label="Select an existing data block to edit or create a new one"
          disabled={isLoading}
          order={[]}
          value={dataBlockId}
          optGroups={{
            'Create data block': [
              {
                label: 'Create new data block',
                value: '',
              },
            ],
            'Edit existing': dataBlockOptions,
          }}
          onChange={e => {
            history.push(
              manageDataBlocksRoute.generateLink({
                publicationId,
                releaseId,
                dataBlockId: e.target.value ? e.target.value : undefined,
              }),
            );
          }}
        />
      )}

      <hr />

      <div style={{ position: 'relative' }}>
        {(isLoading || isSaving) && (
          <LoadingSpinner
            text={`${isSaving ? 'Saving data block' : 'Loading data block'}`}
            overlay
          />
        )}

        <div>
          <h2>
            {dataBlockData && dataBlockData.dataBlock
              ? dataBlockData.dataBlock.name || 'title not set'
              : 'Create new data block'}
          </h2>

          <Tabs
            openId={activeTab}
            onToggle={tab => {
              setActiveTab(tab.id);
            }}
            id="manageDataBlocks"
          >
            <TabsSection
              title={
                dataBlockData && dataBlockData.dataBlock
                  ? 'Update data source'
                  : 'Create data source'
              }
            >
              <p>Configure the data source for the data block</p>

              {dataBlockData && dataBlockData.dataBlock && (
                <>
                  <div className="govuk-!-margin-top-4">
                    <Button
                      type="button"
                      onClick={() =>
                        setDeleteDataBlock(dataBlockData.dataBlock)
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
                onConfirm={() =>
                  deleteDataBlock && onDeleteDataBlock(deleteDataBlock)
                }
                onExit={() => setDeleteDataBlock(undefined)}
                onCancel={() => setDeleteDataBlock(undefined)}
              >
                <p>Are you sure you wish to delete this data block?</p>
              </ModalConfirm>

              <div style={{ overflow: 'hidden' }}>
                <DataBlockSourceWizard
                  dataBlock={dataBlockData?.dataBlock}
                  dataBlockResponse={dataBlockData?.response}
                  releaseId={releaseId}
                  loading={isLoading}
                  onDataBlockSave={onDataBlockSave}
                  onTableToolLoaded={handleTableToolLoaded}
                />
              </div>
            </TabsSection>
            {!isLoading && dataBlockData && (
              <TabsSection title="Configure content">
                <DataBlockContentTabs
                  {...dataBlockData}
                  dataBlockResponse={dataBlockData?.response}
                  onDataBlockSave={onDataBlockSave}
                />
              </TabsSection>
            )}
          </Tabs>
        </div>
      </div>
    </>
  );
};

export default ReleaseManageDataBlocksPage;
