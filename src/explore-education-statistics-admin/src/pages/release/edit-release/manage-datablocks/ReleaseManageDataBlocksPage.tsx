import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import DataBlocksService from '@admin/services/release/edit-release/datablocks/service';
import { DataBlock } from '@admin/services/release/edit-release/datablocks/types';
import Button from '@common/components/Button';
import { FormSelect } from '@common/components/form';
import { SelectOption } from '@common/components/form/FormSelect';
import ModalConfirm from '@common/components/ModalConfirm';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import DataBlockService, {
  DataBlockRequest,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import React, { useContext } from 'react';
import LoadingSpinner from '@common/components/LoadingSpinner';
import CreateDataBlocks from './CreateDataBlocks';
import ViewDataBlocks from './ViewDataBlocks';

const ReleaseManageDataBlocksPage = () => {
  const { releaseId } = useContext(ManageReleaseContext) as ManageRelease;

  const [selectedDataBlock, setSelectedDataBlock] = React.useState<string>('');
  const [dataBlocks, setDataBlocks] = React.useState<DataBlock[]>([]);
  const [dataBlockOptions, setDataBlockOptions] = React.useState<
    SelectOption[]
  >([]);

  const updateDataBlocks = (rId: string) => {
    return DataBlocksService.getDataBlocks(rId).then(blocks => {
      setDataBlocks(blocks);
    });
  };

  React.useEffect(() => {
    updateDataBlocks(releaseId).then(() => {});
  }, [releaseId]);

  React.useEffect(() => {
    setDataBlockOptions(
      dataBlocks.map(({ heading, id }, index) => ({
        label: `${heading || index}`,
        value: `${id}`,
      })),
    );
  }, [dataBlocks]);

  const [isLoading, setIsLoading] = React.useState<boolean>(false);
  const [dataBlock, setDataBlock] = React.useState<DataBlock>();
  const [dataBlockRequest, setDataBlockRequest] = React.useState<DataBlockRequest>();

  const [dataBlockResponse, setDataBlockResponse] = React.useState<DataBlockResponse>();

  const onDataBlockSave = async (db: DataBlock) => {
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
  };

  const [deleteDataBlock, setDeleteDataBlock] = React.useState<DataBlock>();

  const onDeleteDataBlock = (db: DataBlock) => {
    setDeleteDataBlock(undefined);
    if (db.id) {
      DataBlocksService.deleteDataBlock(db.id)
        .then(() => updateDataBlocks(releaseId))
        .then(() => setSelectedDataBlock(''));
    }
  };

  React.useEffect(() => {
    setDataBlockResponse(undefined);
    setDataBlock(undefined);
    setDataBlockRequest(undefined);

    setIsLoading(true);

    Promise.resolve(dataBlocks.find(({ id }) => selectedDataBlock === id))
      .then(db => {
        if (db === undefined) throw new Error();
        setDataBlock(db);

        const request = db.dataBlockRequest;
        if (request === undefined) throw new Error();

        setDataBlockRequest(request);
        return request;
      })
      .then(request => DataBlockService.getDataBlockForSubject(request))
      .then(response => {
        if (response === undefined) throw new Error('undefined response');

        setDataBlockResponse({
          ...response,
          releaseId,
        });
      })
      .catch(() => {
        setDataBlockResponse(undefined);
        setDataBlock(undefined);
        setDataBlockRequest(undefined);
        setIsLoading(false);
      });

    const selectedIndex = Number.parseInt(selectedDataBlock, 10);

    if (!Number.isNaN(selectedIndex) && dataBlocks[selectedIndex]) {
      const request = dataBlocks[selectedIndex].dataBlockRequest;

      if (request) {
        DataBlockService.getDataBlockForSubject(request);
      }
    }
  }, [dataBlocks, releaseId, selectedDataBlock]);

  return (
    <>
      <FormSelect
        id="selectDataBlock"
        name="selectDataBlock"
        label="Select a existing data block to edit or create a new one"
        onChange={e => {
          setSelectedDataBlock(e.target.value);
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
            {dataBlock
              ? dataBlock.heading || 'title not set'
              : 'Create new Data Block'}
          </h2>

          <Tabs id="manageDataBlocks">
            <TabsSection
              title={dataBlock ? 'Update Data source' : 'Create Data source'}
            >
              <p>Configure the data source for the data block</p>

              {dataBlock && (
                <>
                  <div className="govuk-!-margin-top-4">
                    <Button
                      type="button"
                      onClick={() => setDeleteDataBlock(dataBlock)}
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
                  dataBlockRequest={dataBlockRequest}
                  dataBlockResponse={dataBlockResponse}
                  dataBlock={dataBlock}
                  onDataBlockSave={onDataBlockSave}
                  onTableToolLoaded={() => setIsLoading(false)}
                />
              </div>
            </TabsSection>
            {dataBlock && dataBlockResponse && dataBlockRequest && (
              <TabsSection title="Configure Content">
                <ViewDataBlocks
                  dataBlock={dataBlock}
                  dataBlockRequest={dataBlockRequest}
                  dataBlockResponse={dataBlockResponse}
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
