/* eslint-disable no-shadow */
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import DataBlocksService from '@admin/services/release/edit-release/datablocks/service';
import Button from '@common/components/Button';
import { FormSelect } from '@common/components/form';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import DataBlockService, {
  DataBlock,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import React, { useContext } from 'react';
import CreateDataBlocks from './CreateDataBlocks';
import ViewDataBlocks from './ViewDataBlocks';

interface DataBlockData {
  dataBlock: DataBlock;
  dataBlockResponse: DataBlockResponse;
}

const ReleaseManageDataBlocksPage = () => {
  const { releaseId } = useContext(ManageReleaseContext) as ManageRelease;

  const [dataBlocks, setDataBlocks] = React.useState<DataBlock[]>([]);
  const [selectedDataBlock, setSelectedDataBlock] = React.useState<
    DataBlock['id']
  >('');

  const [isLoading, setIsLoading] = React.useState<boolean>(false);
  const [isSaving, setIsSaving] = React.useState<boolean>(false);

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
      dataBlocks.map(({ name, id }, index) => ({
        label: `${name || index}`,
        value: `${id}`,
      })),
    [dataBlocks],
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
    (
      releaseId: string,
      selectedDataBlockId: string | undefined,
      dataBlocks: DataBlock[],
    ) => {
      if (!selectedDataBlockId) {
        setDataBlockData(undefined);
        setIsLoading(false);
        currentlyLoadingDataBlockId.current = undefined;
        return;
      }

      if (currentlyLoadingDataBlockId.current !== selectedDataBlockId) {
        currentlyLoadingDataBlockId.current = selectedDataBlockId;

        load(dataBlocks, releaseId, selectedDataBlockId).then(
          ({ dataBlock, response: dataBlockResponse }) => {
            if (currentlyLoadingDataBlockId.current === selectedDataBlockId) {
              if (dataBlock && dataBlockResponse) {
                setDataBlockData({
                  dataBlock,
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
    [],
  );

  const onDataBlockSave = React.useMemo(
    () => async (db: DataBlock) => {
      setIsSaving(true);

      let newDataBlock;
      let newDataBlocksList;

      if (db.id) {
        newDataBlock = await DataBlocksService.putDataBlock(db.id, db);
        newDataBlocksList = [
          ...dataBlocks.filter(db => db.id !== selectedDataBlock),
          newDataBlock,
        ];
      } else {
        newDataBlock = await DataBlocksService.postDataBlock(releaseId, db);
        newDataBlocksList = [...dataBlocks, newDataBlock];
      }
      setDataBlocks(newDataBlocksList);

      setSelectedDataBlock(newDataBlock.id || '');
      doLoad(releaseId, selectedDataBlock, newDataBlocksList);

      setIsSaving(false);

      return newDataBlock;
    },
    [dataBlocks, doLoad, releaseId, selectedDataBlock],
  );

  React.useEffect(() => {
    doLoad(releaseId, selectedDataBlock, dataBlocks);
  }, [releaseId, dataBlocks, doLoad, selectedDataBlock]);

  return (
    <>
      <FormSelect
        id="selectDataBlock"
        name="selectDataBlock"
        label="Select an existing data block to edit or create a new one"
        onChange={e => {
          setSelectedDataBlock(e.target.value);
        }}
        order={[]}
        optGroups={{
          'Create data block': [
            {
              label: 'Create new data block',
              value: '',
            },
          ],
          'Edit existing': dataBlockOptions,
        }}
        value={selectedDataBlock}
      />

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

          <Tabs id="manageDataBlocks">
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

              {deleteDataBlock && (
                <ModalConfirm
                  title="Delete data block"
                  mounted={deleteDataBlock !== undefined}
                  onConfirm={() => onDeleteDataBlock(deleteDataBlock)}
                  onExit={() => setDeleteDataBlock(undefined)}
                  onCancel={() => setDeleteDataBlock(undefined)}
                >
                  <p>Are you sure you wish to delete this data block?</p>
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
            {!isLoading && dataBlockData && (
              <TabsSection title="Configure content">
                <ViewDataBlocks
                  {...dataBlockData}
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
