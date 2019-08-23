import React, { useContext } from 'react';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import ChartBuilder from '@admin/modules/chart-builder/ChartBuilder';
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import DataBlockService, {
  DataBlockRequest,
  DataBlockRerequest,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import { Chart } from '@common/services/publicationService';
import { ChartRendererProps } from '@common/modules/find-statistics/components/ChartRenderer';
import LoadingSpinner from '@common/components/LoadingSpinner';

const ReleaseManageDataBlocksPage = () => {
  const { publication, releaseId } = useContext(
    ManageReleaseContext,
  ) as ManageRelease;

  const [chartBuilderData, setChartBuilderData] = React.useState<
    DataBlockResponse
  >();

  const [request, setRequest] = React.useState<DataBlockRequest>();

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
        setChartBuilderData(response);
        setInitialConfiguration(requestConfiguration);
      });
    }
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
      <Tabs id="manageDataBlocks">
        <TabsSection title="Create data blocks">something</TabsSection>
        <TabsSection title="View datablocks">
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
        </TabsSection>
      </Tabs>
    </>
  );
};

export default ReleaseManageDataBlocksPage;
