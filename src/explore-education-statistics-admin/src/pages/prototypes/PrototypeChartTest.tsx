import PrototypePage from '@admin/pages/prototypes/components/PrototypePage';
import PrototypeData from '@admin/pages/prototypes/PrototypeData';
import ChartRenderer from '@common/modules/find-statistics/components/ChartRenderer';
import ChartData from '@common/modules/find-statistics/components/charts/__tests__/__data__/testBlockData';
import { StackedBarProps } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import { DataBlockResponse } from '@common/services/dataBlockService';
import React from 'react';

const PrototypeChartTest = () => {
  const [data] = React.useState<DataBlockResponse>(PrototypeData.testResponse);

  const chartData = ChartData.AbstractLargeDataChartProps_smaller_datasets;

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

  return (
    <PrototypePage wide>
      {/*<ChartBuilder data={data} />*/}
      <ChartRenderer type="map" {...newChartData} />
    </PrototypePage>
  );
};

export default PrototypeChartTest;
