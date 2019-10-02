import Link from '@admin/components/Link';
import React, {useContext} from 'react';
import TabsSection from '@common/components/TabsSection';
import Tabs from '@common/components/Tabs';
import CreateDataBlocks from './CreateDataBlocks';
import ViewDataBlocks from './ViewDataBlocks';
import {DataBlock} from "@admin/services/release/edit-release/datablocks/types";
import DataBlocksService from "@admin/services/release/edit-release/datablocks/service";
import ManageReleaseContext, {ManageRelease} from "@admin/pages/release/ManageReleaseContext";
import {FormSelect} from '@common/components/form';
import DataBlockService, {DataBlockResponse, DataBlockRequest} from '@common/services/dataBlockService';
import {SelectOption} from '@common/components/form/FormSelect';

const ReleaseManageDataBlocksPage = () => {

  const {releaseId, lastModified} = useContext(
    ManageReleaseContext,
  ) as ManageRelease;


  const [selectedDataBlock, setSelectedDataBlock] = React.useState<string>('');
  const [dataBlocks, setDataBlocks] = React.useState<DataBlock[]>([]);
  const [dataBlockOptions, setDataBlockOptions] = React.useState<SelectOption[]>([]);

  React.useEffect(() => {
    DataBlocksService.getDataBlocks(releaseId).then(blocks => {
      setDataBlocks(blocks);
    });
  }, [releaseId]);

  React.useEffect(() => {

    setDataBlockOptions(dataBlocks.map(({heading}, index) => ({
      label: `${heading || index}`,
      value: `${index}`,
    })));


  }, [dataBlocks])


  const [dataBlock, setDataBlock] = React.useState<DataBlock>();
  const [dataBlockRequest, setDataBlockRequest] = React.useState<DataBlockRequest>();
  const [dataBlockResponse, setDataBlockResponse] = React.useState<DataBlockResponse>();

  React.useEffect(() => {

    Promise.resolve(Number.parseInt(selectedDataBlock, 10))
      .then(selectedIndex => {
        if (Number.isNaN(selectedIndex)) throw new Error();

        const db = dataBlocks[selectedIndex];
        if (db === undefined) throw new Error();
        setDataBlock(db);

        const request = db.dataBlockRequest;
        if (request === undefined) throw new Error();

        setDataBlockRequest(request);
        return request;
      })
      .then(request => DataBlockService.getDataBlockForSubject(request))
      .then(response => {
        if (response === undefined) throw new Error("undefined response");

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
        DataBlockService.getDataBlockForSubject(request)
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
            "Create Data Block": [
              {
                label: "Create new Data Block",
                value: ""
              }
            ],
            "Edit existing": dataBlockOptions
          }
        }
      />

      <hr />

      <h2>
        {dataBlock ? dataBlock.heading || "title not set" : "Create new Data Block"}
      </h2>

      <Tabs id="manageDataBlocks">
        <TabsSection title={dataBlock ? "Edit data block" : "Create Data block"}>
          <CreateDataBlocks
            dataBlockRequest={dataBlockRequest}
            dataBlockResponse={dataBlockResponse}
          />
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
