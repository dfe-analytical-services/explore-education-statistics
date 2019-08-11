import PrototypePage from '@admin/pages/prototypes/components/PrototypePage';
import ChartData from '@common/modules/find-statistics/components/charts/__tests__/__data__/testBlockData';
import { DataBlockResponse } from '@common/services/dataBlockService';
import React from 'react';
import ChartBuilder from '@admin/modules/chart-builder/ChartBuilder';

const PrototypeChartTest = () => {
  const chartData = ChartData.AbstractLargeDataChartProps_smaller_datasets;

  const newChartBuilderData: DataBlockResponse = {
    ...chartData.data,
    metaData: chartData.meta,
  };

  /* For the ChartRenderer when testing;
  const newChartData: StackedBarProps = {
    ...chartData,
    height: 600,
    axes: {
      ...chartData.axes,
      major: {
        ...chartData.axes.major,
        sortBy: '23_1_2_____',
        sortAsc: false,
        dataRange: [0, 20],
        groupBy: 'locations',
      },
      minor: {
        ...chartData.axes.minor,
        min: '0',
      },
    },
  };
  */

  return (
    <PrototypePage wide>
      <ChartBuilder data={newChartBuilderData} />
      {/*<ChartRenderer type="map" {...newChartData} />*/}
    </PrototypePage>
  );
};

export default PrototypeChartTest;
