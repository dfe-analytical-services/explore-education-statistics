import TableTabSection from '@admin/pages/release/datablocks/components/TableTabSection';
import { ReleaseDataBlock } from '@admin/services/dataBlockService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import ChartRenderer from '@common/modules/charts/components/ChartRenderer';
import { AxesConfiguration } from '@common/modules/charts/types/chart';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import React from 'react';
import tableBuilderService, {
  ReleaseTableDataQuery,
} from '@common/services/tableBuilderService';

interface Model {
  table: FullTable;
  tableHeaders: TableHeadersConfig;
}

interface Props {
  releaseId: string;
  dataBlock: ReleaseDataBlock;
}

const DataBlockPageReadOnlyTabs = ({ releaseId, dataBlock }: Props) => {
  const { value: model, isLoading } = useAsyncRetry<Model>(async () => {
    const query: ReleaseTableDataQuery = {
      ...dataBlock.query,
      releaseId,
      includeGeoJson: dataBlock.charts.some(chart => chart.type === 'map'),
    };

    const tableData = await tableBuilderService.getTableData(query);
    const table = mapFullTable(tableData);

    const tableHeaders = mapTableHeadersConfig(
      dataBlock.table.tableHeaders,
      table,
    );

    return {
      table,
      tableHeaders,
    };
  }, [releaseId, dataBlock]);

  return (
    <LoadingSpinner text="Loading data block" loading={isLoading}>
      {model ? (
        <Tabs id="dataBlockTabs">
          <TabsSection title="Table" key="table" id="dataBlockTabs-table">
            <TableTabSection
              dataBlock={dataBlock}
              table={model.table}
              tableHeaders={model.tableHeaders}
            />
          </TabsSection>
          {dataBlock.charts.length > 0 && [
            <TabsSection title="Chart" key="chart" id="dataBlockTabs-chart">
              <div className="govuk-width-container">
                {dataBlock.charts.map((chart, index) => {
                  const key = index;

                  const axes = { ...chart.axes } as Required<AxesConfiguration>;

                  return (
                    <ChartRenderer
                      {...chart}
                      key={key}
                      id={`dataBlockTabs-chart-${index}`}
                      axes={axes}
                      data={model.table.results}
                      meta={model.table.subjectMeta}
                      source={dataBlock.source}
                    />
                  );
                })}
              </div>
            </TabsSection>,
          ]}
        </Tabs>
      ) : (
        <WarningMessage>
          There was a problem loading the data block
        </WarningMessage>
      )}
    </LoadingSpinner>
  );
};

export default DataBlockPageReadOnlyTabs;
