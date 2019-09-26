import ChartBuilder from '@admin/modules/chart-builder/ChartBuilder';
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import DataBlocksService from '@admin/services/release/edit-release/datablocks/service';
import {DataBlock} from '@admin/services/release/edit-release/datablocks/types';
import FormSelect from '@common/components/form/FormSelect';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import {ChartRendererProps} from '@common/modules/find-statistics/components/ChartRenderer';
import DataBlockService, {
  DataBlockRequest,
  DataBlockRerequest,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import {Chart} from '@common/services/publicationService';
import React, {useContext} from 'react';
import {
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/full-table/types/filters';
import {FullTable} from '@common/modules/full-table/types/fullTable';
import getDefaultTableHeaderConfig from '@common/modules/full-table/utils/tableHeaders';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import {TableHeadersFormValues} from '@common/modules/table-tool/components/TableHeadersForm';

const mapFullTable = (unmappedFullTable: DataBlockResponse): FullTable => {
  const subjectMeta = unmappedFullTable.metaData || {
    indicators: {},
    locations: {},
    timePeriodRange: {},
  };

  return {
    results: unmappedFullTable.result,
    subjectMeta: {
      subjectName: "",
      publicationName: 'Test',
      footnotes: [],
      filters: {},
      ...unmappedFullTable.metaData,
      indicators: Object.values(subjectMeta.indicators).map(
        indicator => new Indicator(indicator),
      ),
      locations: Object.values(subjectMeta.locations).map(
        location => new LocationFilter(location, location.level),
      ),
      timePeriodRange: Object.values(subjectMeta.timePeriods).map(
        timePeriod => new TimePeriodFilter(timePeriod),
      ),
    },
  };
};
const ViewDataBlocks = () => {
  const {releaseId, publication} = useContext(
    ManageReleaseContext,
  ) as ManageRelease;

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [selectedDataBlock, setSelectedDataBlock] = React.useState<string>('');
  const [dataBlocks, setDataBlocks] = React.useState<DataBlock[]>([]);

  React.useEffect(() => {
    DataBlocksService.getDataBlocks(releaseId).then(blocks => {
      setDataBlocks(blocks);
    });
  }, [releaseId]);

  const [chartBuilderData, setChartBuilderData] = React.useState<DataBlockResponse>();

  const [request, setRequest] = React.useState<DataBlockRequest>();

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [requestConfiguration, setRequestConfiguration] = React.useState<Chart | undefined>();
  const [initialConfiguration, setInitialConfiguration] = React.useState<Chart | undefined>();

  const [tableData, setTableData] = React.useState<{ fullTable: FullTable, tableHeadersConfig: TableHeadersFormValues }>();

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

          const fullTable = mapFullTable(response);
          const tableHeadersConfig = getDefaultTableHeaderConfig(fullTable.subjectMeta);

          setTableData({
            fullTable,
            tableHeadersConfig
          });

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
            <TabsSection title="table">
              {tableData && (
                <TimePeriodDataTable
                  {...tableData}
                />
              )}
            </TabsSection>
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
    </>
  );
};

export default ViewDataBlocks;
