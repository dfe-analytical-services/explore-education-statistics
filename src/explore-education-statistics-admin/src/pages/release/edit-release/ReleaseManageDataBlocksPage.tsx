import ChartBuilder from '@admin/modules/chart-builder/ChartBuilder';
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import DataBlocksService from '@admin/services/release/edit-release/datablocks/service';
import { DataBlock } from '@admin/services/release/edit-release/datablocks/types';
import FormSelect from '@common/components/form/FormSelect';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import { ChartRendererProps } from '@common/modules/find-statistics/components/ChartRenderer';
import DataBlockService, {
  DataBlockRequest,
  DataBlockRerequest,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import { Chart } from '@common/services/publicationService';
import React, { useContext } from 'react';
import Link from '@admin/components/Link';

const ReleaseManageDataBlocksPage = () => {
  const { releaseId } = useContext(ManageReleaseContext) as ManageRelease;

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [selectedDataBlock, setSelectedDataBlock] = React.useState<string>('');
  const [dataBlocks, setDataBlocks] = React.useState<DataBlock[]>([]);

  React.useEffect(() => {
    DataBlocksService.getDataBlocks(releaseId).then(blocks => {
      setDataBlocks(blocks);
    });
  }, [releaseId]);

  const [chartBuilderData, setChartBuilderData] = React.useState<
    DataBlockResponse
  >();

  const [request, setRequest] = React.useState<DataBlockRequest>();

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [requestConfiguration, setRequestConfiguration] = React.useState<
    Chart | undefined
  >();
  const [initialConfiguration, setInitialConfiguration] = React.useState<
    Chart | undefined
  >();

  React.useEffect(() => {
    // destroy the existing setup before the response completes
    setChartBuilderData(undefined);
    setInitialConfiguration(undefined);

    if (request) {
      DataBlockService.getDataBlockForSubject(request).then(response => {
        if (response) {
          setChartBuilderData({
            ...response,
            releaseId,
          });
          setInitialConfiguration(requestConfiguration);
        }
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [request, requestConfiguration]);

  // eslint-disable-next-line
  const onChartSave = (props: ChartRendererProps) => {
    // console.log('Saved ', props);
  };

  const reRequestdata = (reRequest: DataBlockRerequest) => {
    if (request) {
      setRequest({
        ...request,
        ...reRequest,
      });
    }
  };

  return (
    <>
      <div className="govuk-inset-text">
        <h3>This functionality is still in development.</h3>
        <p>
          We're currently working on embedding a version of the table tool
          filtered to data uploaded from this release within the page. This will
          then allow the creation of data blocks and the ability to create
          charts and tables.
        </p>
        <p>
          While this work is in progress we have added a temporary table tool
          page that allows access to data you have uploaded.
        </p>
        <Link to="/prototypes/table-tool" target="_blank">
          Temporary admin table tool
        </Link>
      </div>
      <Tabs id="manageDataBlocks">
        <TabsSection title="Create data blocks">
          Currently in developent
        </TabsSection>
        <TabsSection title="View datablocks">
          <FormSelect
            id="selectDataBlock"
            name="selectDataBlock"
            label="Select data block"
            onChange={e => {
              setRequest(dataBlocks[+e.target.value].dataBlockRequest);
              setRequestConfiguration(
                (dataBlocks[+e.target.value].charts || [undefined])[0],
              );
              setSelectedDataBlock(e.target.value);
            }}
            order={[]}
            options={[
              {
                label: 'select',
                value: '',
              },
              ...dataBlocks.map((dataBlock, index) => ({
                label: `${index} ${dataBlock.heading}`,
                value: `${index}`,
              })),
            ]}
          />

          {selectedDataBlock && (
            <>
              <hr />
              <Tabs id="editDataBlockSections">
                <TabsSection title="table">Table</TabsSection>
                <TabsSection title="Create Chart">
                  {chartBuilderData ? (
                    <ChartBuilder
                      data={chartBuilderData}
                      onChartSave={onChartSave}
                      initialConfiguration={initialConfiguration}
                      onRequiresDataUpdate={reRequestdata}
                    />
                  ) : (
                    <LoadingSpinner />
                  )}
                </TabsSection>
              </Tabs>
            </>
          )}
        </TabsSection>
      </Tabs>
    </>
  );
};

export default ReleaseManageDataBlocksPage;
