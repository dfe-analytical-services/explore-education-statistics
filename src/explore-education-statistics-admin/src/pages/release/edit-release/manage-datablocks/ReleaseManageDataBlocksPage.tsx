import ManageReleaseContext, { ManageRelease } from '@admin/pages/release/ManageReleaseContext';
import DataBlocksService from '@admin/services/release/edit-release/datablocks/service';
import { DataBlock } from '@admin/services/release/edit-release/datablocks/types';
import Button from '@common/components/Button';
import { FormSelect } from '@common/components/form';
import { SelectOption } from '@common/components/form/FormSelect';
import ModalConfirm from '@common/components/ModalConfirm';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import DataBlockService, { DataBlockRequest, DataBlockResponse } from '@common/services/dataBlockService';
import React, { useContext } from 'react';
import CreateDataBlocks from './CreateDataBlocks';
import ViewDataBlocks from './ViewDataBlocks';

const ReleaseManageDataBlocksPage = () => {

  const { releaseId: r } = useContext(
    ManageReleaseContext,
  ) as ManageRelease;

  const [releaseId, setReleaseId] = React.useState<string>(r);


  const [selectedDataBlock, setSelectedDataBlock] = React.useState<string>('');
  const [dataBlocks, setDataBlocks] = React.useState<DataBlock[]>([]);
  const [dataBlockOptions, setDataBlockOptions] = React.useState<SelectOption[]>([]);

  const updateDataBlocks = (rId: string) => {
    return DataBlocksService.getDataBlocks(rId).then(blocks => {
      setDataBlocks(blocks);
    });
  };

  React.useEffect(() => {
    updateDataBlocks(releaseId).then(() => {
    });
  }, [releaseId]);

  React.useEffect(() => {

    setDataBlockOptions(dataBlocks.map(({ heading, id }, index) => ({
      label: `${heading || index}`,
      value: `${id}`,
    })));


  }, [dataBlocks]);


  const [dataBlock, setDataBlock] = React.useState<DataBlock>();
  const [dataBlockRequest, setDataBlockRequest] = React.useState<DataBlockRequest>();
  const [dataBlockResponse, setDataBlockResponse] = React.useState<DataBlockResponse>();

  const onDataBlockSave = async (db: DataBlock) => {
    const newDataBlock = await DataBlocksService.postDataBlock(releaseId, db);

    if (db.id !== selectedDataBlock) {
      updateDataBlocks(releaseId)
        .then(() => {
          setSelectedDataBlock(db.id || '');
        });

    }

    return newDataBlock;
  };

  const [deleteDataBlock, setOnDeleteDataBlock] = React.useState<DataBlock>();

  const onDeleteDataBlock = (db: DataBlock) => {
    console.log(db);
    setOnDeleteDataBlock(undefined);
  };

  React.useEffect(() => {


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
        optGroups={
          {
            'Create Data Block': [
              {
                label: 'Create new Data Block',
                value: '',
              },
            ],
            'Edit existing': dataBlockOptions,
          }
        }
      />

      <hr />

      <h2>
        {dataBlock ? dataBlock.heading || 'title not set' : 'Create new Data Block'}
      </h2>


      <Tabs id="manageDataBlocks">
        <TabsSection title={dataBlock ? 'Update Data source' : 'Create Data source'}>

          <p>Configure the data source for the data block</p>

          {dataBlock && (
            <>
              <div className="govuk-!-margin-top-4">
                <Button
                  type="button"
                  onClick={() => setOnDeleteDataBlock(dataBlock)}
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
              onExit={() => setOnDeleteDataBlock(undefined)}
              onCancel={() => setOnDeleteDataBlock(undefined)}
            >
              <p>
                Are you sure you wish to delete this Data Block?
              </p>
            </ModalConfirm>
          )}


          <div style={{ overflow: 'hidden' }}>
            <CreateDataBlocks
              dataBlockRequest={dataBlockRequest}
              dataBlockResponse={dataBlockResponse}
              onDataBlockSave={onDataBlockSave}
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
    </>
  );
};

export default ReleaseManageDataBlocksPage;
