/* eslint-disable no-shadow */
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import dataBlocksService from '@admin/services/release/edit-release/datablocks/service';
import permissionService from '@admin/services/permissions/service';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
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
import React, { useContext } from 'react';
import CreateDataBlocks from './CreateDataBlocks';
import ViewDataBlocks from './ViewDataBlocks';

interface DataBlockData {
  dataBlock: DataBlock;
  dataBlockResponse: DataBlockResponse;
}

const ReleaseManageDataBlocksPage = ({
  handleApiErrors,
}: ErrorControlProps) => {
  const { releaseId } = useContext(ManageReleaseContext) as ManageRelease;

  const [dataBlocks, setDataBlocks] = React.useState<DataBlock[]>([]);
  const [selectedDataBlock, setSelectedDataBlock] = React.useState<
    DataBlock['id']
  >('');

  const [isLoading, setIsLoading] = React.useState<boolean>(false);
  const [isSaving, setIsSaving] = React.useState<boolean>(false);

  const [canUpdateRelease, setCanUpdateRelease] = React.useState(false);

  const [dataBlockData, setDataBlockData] = React.useState<DataBlockData>();

  const [deleteDataBlock, setDeleteDataBlock] = React.useState<DataBlock>();

  const updateDataBlocks = (rId: string) => {
    return dataBlocksService
      .getDataBlocks(rId)
      .then(setDataBlocks)
      .catch(handleApiErrors);
  };

  React.useEffect(() => {
    updateDataBlocks(releaseId);
  }, [releaseId]);

  React.useEffect(() => {
    permissionService
      .canUpdateRelease(releaseId)
      .then(canUpdateRelease => setCanUpdateRelease(canUpdateRelease));
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
      dataBlocksService
        .deleteDataBlock(db.id)
        .then(() => updateDataBlocks(releaseId))
        .then(() => setSelectedDataBlock(''))
        .catch(handleApiErrors);
    }
  };

  const load = async (
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
    [],
  );

  const onDataBlockSave = React.useMemo(
    () => async (db: DataBlock) => {
      setIsSaving(true);

      let newDataBlock;
      let newDataBlocksList;

      if (db.id) {
        newDataBlock = await dataBlocksService
          .putDataBlock(db.id, db)
          .catch(handleApiErrors);
        newDataBlocksList = [
          ...dataBlocks.filter(db => db.id !== selectedDataBlock),
          newDataBlock,
        ];
      } else {
        newDataBlock = await dataBlocksService
          .postDataBlock(releaseId, db)
          .catch(handleApiErrors);
        newDataBlocksList = [...dataBlocks, newDataBlock];
      }
      setDataBlocks(newDataBlocksList);

      setSelectedDataBlock(newDataBlock.id || '');
      doLoad(releaseId, selectedDataBlock, newDataBlocksList);

      setIsSaving(false);

      return newDataBlock;
    },
    [dataBlocks, doLoad, releaseId, selectedDataBlock, handleApiErrors],
  );

  React.useEffect(() => {
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
  ) : (
    <div>This release is currently not editable.</div>
  );
};

export default withErrorControl(ReleaseManageDataBlocksPage);
