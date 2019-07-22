/* eslint-disable @typescript-eslint/no-unused-vars */
import ChartBuilder from '@admin/modules/chart-builder/ChartBuilder';
import React from 'react';

import LineChartBlock from '@common/modules/find-statistics/components/charts/LineChartBlock';
import PrototypePage from '@admin/pages/prototypes/components/PrototypePage';

import TestBlockData from '@common/modules/find-statistics/components/charts/__tests__/__data__/testBlockData';
import PrototypeData from '@admin/pages/prototypes/PrototypeData';
import { DataBlockResponse } from '@common/services/dataBlockService';

const PrototypeChartTest = () => {
  const [data] = React.useState<DataBlockResponse>(PrototypeData.testResponse);

  return (
    <PrototypePage wide>
      {/*<LineChartBlock
        {...{
          ...TestBlockData.AbstractChartProps,
          labels: {
            ...TestBlockData.AbstractChartProps.labels,

            '23_1_2_____': {
              ...TestBlockData.AbstractChartProps.labels['23_1_2_____'],
              symbol: 'square',
              lineStyle: 'dashed',
            },
          },

          axes: {
            ...TestBlockData.AbstractChartProps.axes,
            major: {
              ...TestBlockData.AbstractChartProps.axes.major,
              referenceLines: [{
                position: "2014/15",
                label: "HELLO WORLD"
              }]
            },
            minor: {
              ...TestBlockData.AbstractChartProps.axes.minor,
              referenceLines: [{
                position: 2.5,
                label: "GoodBYE"
              }]
            }
          }
        }}
      />*/}

      <ChartBuilder data={data} />
    </PrototypePage>
  );
};

export default PrototypeChartTest;
