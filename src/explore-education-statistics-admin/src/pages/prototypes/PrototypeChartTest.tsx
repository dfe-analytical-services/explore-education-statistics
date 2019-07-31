/* eslint-disable @typescript-eslint/no-unused-vars */
import ChartBuilder from '@admin/modules/chart-builder/ChartBuilder';
import React from 'react';

import PrototypePage from '@admin/pages/prototypes/components/PrototypePage';

import PrototypeData from '@admin/pages/prototypes/PrototypeData';
import TestBlockData from '@common/modules/find-statistics/components/charts/__tests__/__data__/testBlockData';
import { DataBlockResponse } from '@common/services/dataBlockService';
import ChartRenderer from '@common/modules/find-statistics/components/ChartRenderer';
import { ChartProps } from '@common/modules/find-statistics/components/charts/ChartFunctions';

const PrototypeChartTest = () => {
  const [data] = React.useState<DataBlockResponse>(PrototypeData.testResponse);

  const chartData = TestBlockData.AbstractChartProps;

  const newChartData: ChartProps = {
    ...chartData,
    legend: 'top',
    legendHeight: '50',
  };

  return (
    <PrototypePage wide>
      <ChartBuilder data={data} />

      <ChartRenderer type="line" {...newChartData} />
    </PrototypePage>
  );
};

export default PrototypeChartTest;
