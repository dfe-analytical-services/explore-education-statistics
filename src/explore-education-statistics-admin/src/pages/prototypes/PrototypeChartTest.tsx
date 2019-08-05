/* eslint-disable @typescript-eslint/no-unused-vars */
import ChartBuilder from '@admin/modules/chart-builder/ChartBuilder';
import React from 'react';

import PrototypePage from '@admin/pages/prototypes/components/PrototypePage';

import PrototypeData from '@admin/pages/prototypes/PrototypeData';
import ChartData from '@common/modules/find-statistics/components/charts/__tests__/__data__/testBlockData';
import {DataBlockResponse} from '@common/services/dataBlockService';
import {
  ChartProps,
  StackedBarProps,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import ChartRenderer from '@common/modules/find-statistics/components/ChartRenderer';

const PrototypeChartTest = () => {
    const [data] = React.useState<DataBlockResponse>(PrototypeData.testResponse);

    const chartData = ChartData.AbstractLargeDataChartProps_smaller_datasets;

    const newChartData: StackedBarProps = {
      ...chartData,
      stacked: true,
      axes: {
        ...chartData.axes,
      },
    };

    return (
      <PrototypePage wide>
        <ChartBuilder data={data} />

      </PrototypePage>
    );
  }
;

export default PrototypeChartTest;
