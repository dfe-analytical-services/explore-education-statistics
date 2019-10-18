/* eslint-disable no-shadow */
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import DataBlocksService from '@admin/services/release/edit-release/datablocks/service';
import { DataBlock } from '@admin/services/release/edit-release/datablocks/types';
import Button from '@common/components/Button';
import { FormSelect } from '@common/components/form';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import DataBlockService, {
  DataBlockRequest,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import React, { useContext } from 'react';
import CreateDataBlocks from './CreateDataBlocks';
import ViewDataBlocks from './ViewDataBlocks';

interface DataBlockData {
  dataBlock: DataBlock;
  dataBlockRequest: DataBlockRequest;
  dataBlockResponse: DataBlockResponse;
}

const ReleaseManageDataBlocksPage = () => {
  const { releaseId } = useContext(ManageReleaseContext) as ManageRelease;

  const [selectedDataBlock, setSelectedDataBlock] = React.useState<string>('');
  const [dataBlocks, setDataBlocks] = React.useState<DataBlock[]>([]);

  const [isLoading, setIsLoading] = React.useState<boolean>(false);

  const [dataBlockData, setDataBlockData] = React.useState<DataBlockData>();

  const [deleteDataBlock, setDeleteDataBlock] = React.useState<DataBlock>();

  const updateDataBlocks = (rId: string) => {
    return DataBlocksService.getDataBlocks(rId).then(blocks => {
      setDataBlocks(blocks);
    });
  };

  React.useEffect(() => {
    updateDataBlocks(releaseId).then(() => {});
  }, [releaseId]);

  const dataBlockOptions = React.useMemo(
    () =>
      dataBlocks.map(({ heading, id }, index) => ({
        label: `${heading || index}`,
        value: `${id}`,
      })),
    [dataBlocks],
  );

  const onDataBlockSave = React.useMemo(
    () => async (db: DataBlock) => {
      let newDataBlock;

      if (db.id) {
        newDataBlock = await DataBlocksService.putDataBlock(db.id, db);
      } else {
        newDataBlock = await DataBlocksService.postDataBlock(releaseId, db);
      }

      if (db.id !== selectedDataBlock) {
        updateDataBlocks(releaseId).then(() => {
          setSelectedDataBlock(db.id || '');
        });
      }

      return newDataBlock;
    },
    [releaseId, selectedDataBlock],
  );

  const onDeleteDataBlock = (db: DataBlock) => {
    setDeleteDataBlock(undefined);
    if (db.id) {
      DataBlocksService.deleteDataBlock(db.id)
        .then(() => updateDataBlocks(releaseId))
        .then(() => setSelectedDataBlock(''));
    }
  };

  const load = async (
    dataBlocks: DataBlock[],
    releaseId: string,
    selectedDataBlockId: string,
  ) => {
    setIsLoading(true);

    if (!selectedDataBlockId) return {};

    const db = dataBlocks.find(({ id }) => selectedDataBlockId === id);

    if (db === undefined) return {};

    const request = db.dataBlockRequest;
    if (request === undefined) return {};

    const response = await DataBlockService.getDataBlockForSubject(request);

    if (response === undefined) return {};

    return {
      dataBlock: db,
      request,
      response: {
        ...response,
        releaseId,
      },
    };
  };

  const currentlyLoadingDataBlockId = React.useRef<string>();

  const unsetIsLoading = React.useCallback(() => {
    setIsLoading(false);
    currentlyLoadingDataBlockId.current = undefined;
  }, []);

  const doLoad = React.useCallback(
    (selectedDataBlockId: string) => {
      if (currentlyLoadingDataBlockId.current !== selectedDataBlockId) {
        currentlyLoadingDataBlockId.current = selectedDataBlockId;

        load(dataBlocks, releaseId, selectedDataBlockId).then(
          ({
            dataBlock,
            request: dataBlockRequest,
            response: dataBlockResponse,
          }) => {
            if (currentlyLoadingDataBlockId.current === selectedDataBlockId) {
              if (dataBlock && dataBlockRequest && dataBlockResponse) {
                setDataBlockData({
                  dataBlock,
                  dataBlockRequest,
                  dataBlockResponse,
                });
              } else {
                setDataBlockData(undefined);
                setIsLoading(false);
                currentlyLoadingDataBlockId.current = undefined;
              }
            }
          },
        );
      }
    },
    [dataBlocks, releaseId],
  );

  return (
    <>
      <FormSelect
        id="selectDataBlock"
        name="selectDataBlock"
        label="Select a existing data block to edit or create a new one"
        onChange={e => {
          setSelectedDataBlock(e.target.value);

          doLoad(e.target.value);
        }}
        order={[]}
        optGroups={{
          'Create Data Block': [
            {
              label: 'Create new Data Block',
              value: '',
            },
          ],
          'Edit existing': dataBlockOptions,
        }}
      />

      <hr />

      <div style={{ position: 'relative' }}>
        {isLoading && <LoadingSpinner text="Loading Data block" overlay />}

        <div>
          <h2>
            {dataBlockData && dataBlockData.dataBlock
              ? dataBlockData.dataBlock.heading || 'title not set'
              : 'Create new Data Block'}
          </h2>

          <Tabs id="manageDataBlocks">
            <TabsSection
              title={
                dataBlockData && dataBlockData.dataBlock
                  ? 'Update Data source'
                  : 'Create Data source'
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

              {deleteDataBlock && (
                <ModalConfirm
                  title="Delete Data Block"
                  mounted={deleteDataBlock !== undefined}
                  onConfirm={() => onDeleteDataBlock(deleteDataBlock)}
                  onExit={() => setDeleteDataBlock(undefined)}
                  onCancel={() => setDeleteDataBlock(undefined)}
                >
                  <p>Are you sure you wish to delete this Data Block?</p>
                </ModalConfirm>
              )}

              <div style={{ overflow: 'hidden' }}>
                <CreateDataBlocks
                  releaseId={releaseId}
                  {...dataBlockData}
                  onDataBlockSave={onDataBlockSave}
                  onTableToolLoaded={unsetIsLoading}
                />
              </div>
            </TabsSection>
            {dataBlockData && (
              <TabsSection title="Configure Content">
                <ViewDataBlocks {...dataBlockData} />
              </TabsSection>
            )}
          </Tabs>
        </div>
      </div>
    </>
  );
};

export default ReleaseManageDataBlocksPage;
