/* eslint-disable no-shadow */
import { ErrorControlState } from '@admin/contexts/ErrorControlContext';
import withErrorControl from '@admin/hocs/withErrorControl';
import DataBlockContentTabs from '@admin/pages/release/edit-release/manage-datablocks/components/DataBlockContentTabs';
import DataBlockSourceWizard from '@admin/pages/release/edit-release/manage-datablocks/components/DataBlockSourceWizard';
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import permissionService from '@admin/services/permissions/service';
import dataBlocksService from '@admin/services/release/edit-release/datablocks/service';
import Button from '@common/components/Button';
import { FormSelect } from '@common/components/form';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import dataBlockService, {
  DataBlock,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import React, {
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';

interface DataBlockData {
  dataBlock: DataBlock;
  dataBlockResponse: DataBlockResponse;
}

const ReleaseManageDataBlocksPage = ({
  handleApiErrors,
}: ErrorControlState) => {
  const { releaseId } = useContext(ManageReleaseContext) as ManageRelease;

  const [dataBlocks, setDataBlocks] = useState<DataBlock[]>([]);
  const [activeTab, setActiveTab] = useState<string>('');
  const [selectedDataBlock, setSelectedDataBlock] = useState<DataBlock['id']>(
    '',
  );

  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [isSaving, setIsSaving] = useState<boolean>(false);

  const [canUpdateRelease, setCanUpdateRelease] = useState(false);

  const [dataBlockData, setDataBlockData] = useState<DataBlockData>();

  const [deleteDataBlock, setDeleteDataBlock] = useState<DataBlock>();

  const updateDataBlocks = useCallback(
    (rId: string) => {
      return dataBlocksService
        .getDataBlocks(rId)
        .then(setDataBlocks)
        .catch(handleApiErrors);
    },
    [handleApiErrors],
  );

  useEffect(() => {
    updateDataBlocks(releaseId);
  }, [releaseId, updateDataBlocks]);

  useEffect(() => {
    permissionService
      .canUpdateRelease(releaseId)
      .then(canUpdateRelease => setCanUpdateRelease(canUpdateRelease));
  }, [releaseId]);

  const dataBlockOptions = useMemo(
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
      dataBlocksService
        .deleteDataBlock(db.id)
        .then(() => updateDataBlocks(releaseId))
        .then(() => setSelectedDataBlock(''))
        .catch(handleApiErrors);
    }
  };

  const load = useCallback(
    async (
      dataBlocks: DataBlock[],
      releaseId: string,
      selectedDataBlockId: string,
    ) => {
      // Load's the datablock of 'selectedDataBlockId' in datablock form

      setIsLoading(true);

      if (!selectedDataBlockId) return {};

      const db = dataBlocks.find(({ id }) => selectedDataBlockId === id);

      if (db === undefined) return {};

      const request = db.dataBlockRequest;
      if (request === undefined) return {};

      const response = await dataBlockService
        .getDataBlockForSubject(request)
        .catch(handleApiErrors);

      if (response === undefined) return {};

      return {
        dataBlock: db,
        request,
        response: {
          ...response,
          releaseId,
        },
      };
    },
    [handleApiErrors],
  );

  const currentlyLoadingDataBlockId = useRef<string>();

  const unsetIsLoading = useCallback(() => {
    setIsLoading(false);
    currentlyLoadingDataBlockId.current = undefined;
  }, []);

  const doLoad = useCallback(
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
            if (dataBlock && dataBlockResponse) {
              if (dataBlock.id === selectedDataBlockId) {
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
    [load],
  );

  const onDataBlockSave = useMemo(
    () => async (db: DataBlock): Promise<DataBlock> => {
      setIsSaving(true);

      try {
        let newDataBlock;
        let newDataBlocksList;

        if (db.id) {
          newDataBlock = await dataBlocksService.putDataBlock(db.id, db);
          newDataBlocksList = [
            ...dataBlocks.filter(db => db.id !== selectedDataBlock),
            newDataBlock,
          ];
        } else {
          newDataBlock = await dataBlocksService.postDataBlock(releaseId, db);
          newDataBlocksList = [...dataBlocks, newDataBlock];
        }
        setDataBlocks(newDataBlocksList);

        setSelectedDataBlock(newDataBlock.id || '');

        doLoad(releaseId, selectedDataBlock, newDataBlocksList);

        setIsSaving(false);

        return newDataBlock;
      } catch (err) {
        handleApiErrors(err);
        throw err;
      }
    },
    [dataBlocks, doLoad, releaseId, selectedDataBlock, handleApiErrors],
  );

  useEffect(() => {
    doLoad(releaseId, selectedDataBlock, dataBlocks);
  }, [releaseId, dataBlocks, doLoad, selectedDataBlock]);

  return canUpdateRelease ? (
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
                  {...dataBlockData}
                  releaseId={releaseId}
                  loading={isLoading}
                  onDataBlockSave={onDataBlockSave}
                  onTableToolLoaded={unsetIsLoading}
                />
              </div>
            </TabsSection>
            {!isLoading && dataBlockData && (
              <TabsSection title="Configure content">
                <DataBlockContentTabs
                  {...dataBlockData}
                  onDataBlockSave={onDataBlockSave}
                />
              </TabsSection>
            )}
          </Tabs>
        </div>
      </div>
    </>
  ) : (
    <div>This release is currently not editable.</div>
  );
};

export default withErrorControl(ReleaseManageDataBlocksPage);
