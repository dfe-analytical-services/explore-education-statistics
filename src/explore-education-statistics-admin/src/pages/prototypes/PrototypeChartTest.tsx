import PrototypePage from '@admin/pages/prototypes/components/PrototypePage';
import ChartData from '@common/modules/find-statistics/components/charts/__tests__/__data__/testBlockData';
import { DataBlockResponse } from '@common/services/dataBlockService';
import React from 'react';
import ChartBuilder from '@admin/modules/chart-builder/ChartBuilder';
import { ChartRendererProps } from '@common/modules/find-statistics/components/ChartRenderer';

const PrototypeChartTest = () => {
  const chartData = ChartData.AbstractLargeDataChartProps_smaller_datasets;

  const newChartBuilderData: DataBlockResponse = {
    ...chartData.data,
    metaData: chartData.meta,
  };

  /* const newChartData: StackedBarProps = {
     ...ChartData.AbstractChartProps,
   };*/

  const onChartSave = (props : ChartRendererProps) => {

    console.log("Saved " , props);

  };

  return (
    <PrototypePage wide>
      <ChartBuilder data={newChartBuilderData} onChartSave={onChartSave} />
      {/* <ChartRenderer type="line" {...newChartData} /> */}
    </PrototypePage>
  );
};

export default PrototypeChartTest;
