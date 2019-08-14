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

  /* const newChartData: StackedBarProps = {
     ...ChartData.AbstractChartProps,
   };*/

  return (
    <PrototypePage wide>
      <ChartBuilder data={newChartBuilderData} />
      {/* <ChartRenderer type="line" {...newChartData} /> */}
    </PrototypePage>
  );
};

export default PrototypeChartTest;
