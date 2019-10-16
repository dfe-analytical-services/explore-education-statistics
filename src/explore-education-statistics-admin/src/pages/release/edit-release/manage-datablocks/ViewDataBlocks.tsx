import {
  mapFullTable,
  } from '@admin/pages/release/edit-release/manage-datablocks/tableUtil';
import { DataBlock } from '@admin/services/release/edit-release/datablocks/types';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import { ChartRendererProps } from '@common/modules/find-statistics/components/ChartRenderer';
import { FullTable } from '@common/modules/full-table/types/fullTable';
import getDefaultTableHeaderConfig from '@common/modules/full-table/utils/tableHeaders';
import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import DataBlockService, {
  DataBlockRequest,
  DataBlockRerequest,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import { Chart } from '@common/services/publicationService';
import React from 'react';
import { reverseMapTableHeadersConfig } from '@common/modules/table-tool/components/utils/tableToolHelpers';

interface Props {
  dataBlock: DataBlock;
  dataBlockRequest: DataBlockRequest;
  dataBlockResponse: DataBlockResponse;
}

const ViewDataBlocks = ({
  dataBlock,
  dataBlockResponse,
  dataBlockRequest,
}: Props) => {
  // we want to modify this internally as our own data, copying it
  const [chartBuilderData, setChartBuilderData] = React.useState<DataBlockResponse>(() => {
    return { ...dataBlockResponse };
  });

  // only update it if the external reference changes
  React.useEffect(() => {
    setChartBuilderData({ ...dataBlockResponse });
  }, [dataBlockResponse]);

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [initialConfiguration, setInitialConfiguration] = React.useState<Chart | undefined>();

  React.useEffect(() => {
    if (dataBlock && dataBlock.charts) {
      setInitialConfiguration({
        ...dataBlock.charts[0],
      });
    }
  }, [dataBlock]);

  const [tableData, setTableData] = React.useState<{
    fullTable: FullTable;
    tableHeadersConfig: TableHeadersFormValues;
  }>();

  React.useEffect(() => {
    const table = dataBlock.tables;
    const fullTable = mapFullTable(chartBuilderData);
    const tableHeadersConfig =
      (table && table[0].tableHeaders) ||
      getDefaultTableHeaderConfig(fullTable.subjectMeta);

    setTableData({
      fullTable,
      tableHeadersConfig: reverseMapTableHeadersConfig(
        tableHeadersConfig,
        fullTable.subjectMeta,
      ),
    });
  }, [dataBlock.tables, chartBuilderData]);

  // eslint-disable-next-line
  const onChartSave = (props: ChartRendererProps) => {
    // console.log('Saved ', props);
  };

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const reRequestdata = (reRequest: DataBlockRerequest) => {
    const newRequest = {
      ...dataBlockRequest,
      ...reRequest,
    };

    DataBlockService.getDataBlockForSubject(newRequest).then(response => {
      if (response) {
        setChartBuilderData({ ...response });
      }
    });
  };

  return (
    <>
      <Tabs id="editDataBlockSections">
        <TabsSection title="table">
          {tableData && <TimePeriodDataTable {...tableData} />}
        </TabsSection>
        {/*
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
        */}
      </Tabs>
    </>
  );
};

export default ViewDataBlocks;
