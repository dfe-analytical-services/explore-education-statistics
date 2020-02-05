import ChartBuilder from '@admin/modules/chart-builder/ChartBuilder';
import { mapFullTable } from '@admin/pages/release/edit-release/manage-datablocks/tableUtil';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import { ChartRendererProps } from '@common/modules/find-statistics/components/ChartRenderer';
import { FullTable } from '@common/modules/full-table/types/fullTable';
import getDefaultTableHeaderConfig, {
  TableHeadersConfig,
} from '@common/modules/full-table/utils/tableHeaders';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import mapTableHeadersConfig from '@common/modules/table-tool/components/utils/mapTableHeadersConfig';
import dataBlockService, {
  DataBlock,
  DataBlockRerequest,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import { Chart, ChartType } from '@common/services/publicationService';
import React, { useEffect, useState } from 'react';

interface Props {
  dataBlock: DataBlock;
  dataBlockResponse: DataBlockResponse;
  onDataBlockSave: (db: DataBlock) => Promise<DataBlock>;
}

const ViewDataBlocks = ({
  dataBlock,
  dataBlockResponse,
  onDataBlockSave,
  handleApiErrors,
}: Props & ErrorControlProps) => {
  // we want to modify this internally as our own data, copying it
  const [chartBuilderData, setChartBuilderData] = useState<DataBlockResponse>(
    () => {
      return { ...dataBlockResponse };
    },
  );

  // only update it if the external reference changes
  useEffect(() => {
    setChartBuilderData({ ...dataBlockResponse });
  }, [dataBlockResponse]);

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [initialConfiguration, setInitialConfiguration] = useState<
    Chart | undefined
  >();

  useEffect(() => {
    if (dataBlock && dataBlock.charts) {
      setInitialConfiguration({
        ...dataBlock.charts[0],
      });
    }
  }, [dataBlock]);

  const [tableData, setTableData] = useState<{
    fullTable: FullTable;
    tableHeadersConfig: TableHeadersConfig;
  }>();

  useEffect(() => {
    const table = dataBlock.tables;
    const fullTable = mapFullTable(chartBuilderData);
    const tableHeadersConfig =
      (table && table.length > 0 && table[0].tableHeaders) ||
      getDefaultTableHeaderConfig(fullTable.subjectMeta);

    setTableData({
      fullTable,
      tableHeadersConfig: mapTableHeadersConfig(
        tableHeadersConfig,
        fullTable.subjectMeta,
      ),
    });
  }, [dataBlock.tables, chartBuilderData]);

  const onChartSave = (props: ChartRendererProps) => {
    // copy and strip out redundant data from the properties
    const chart: Chart = {
      type: props.type as ChartType,
      axes: props.axes,
      fileId: props.fileId,
      geographicId: props.geographicId,
      height: props.height,
      labels: props.labels,
      legend: props.legend,
      legendHeight: props.legendHeight,
      stacked: props.stacked,
      title: props.title,
      width: props.width,
    };

    const newDataBlock: DataBlock = {
      ...dataBlock,
      charts: [chart],
    };

    return onDataBlockSave(newDataBlock).then(() => {
      return { ...props };
    });
  };

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const reRequestdata = (reRequest: DataBlockRerequest) => {
    const newRequest = {
      ...dataBlock.dataBlockRequest,
      ...reRequest,
    };

    dataBlockService
      .getDataBlockForSubject(newRequest)
      .then(response => {
        if (response) {
          setChartBuilderData({ ...response });
        }
      })
      .catch(handleApiErrors);
  };

  return (
    <>
      <Tabs id="editDataBlockSections">
        <TabsSection title="Table">
          <div className="govuk-width-container">
            {tableData && <TimePeriodDataTable {...tableData} />}
          </div>
        </TabsSection>
        <TabsSection title="Create Chart">
          {chartBuilderData ? (
            <div style={{ position: 'relative' }}>
              <ChartBuilder
                data={chartBuilderData}
                onChartSave={onChartSave}
                initialConfiguration={initialConfiguration}
                onRequiresDataUpdate={reRequestdata}
              />
            </div>
          ) : (
            <LoadingSpinner text="Creating chart" />
          )}
        </TabsSection>
      </Tabs>
    </>
  );
};

export default withErrorControl(ViewDataBlocks);
