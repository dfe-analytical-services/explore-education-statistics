import React from 'react';

import LineChartBlock from '@common/modules/find-statistics/components/charts/LineChartBlock';
import PrototypePage from '@admin/pages/prototypes/components/PrototypePage';

import Data from '@common/modules/find-statistics/components/charts/__tests__/__data__/testBlockData';
import HorizontalBarBlock from '@common/modules/find-statistics/components/charts/HorizontalBarBlock';
import VerticalBarBlock from '@common/modules/find-statistics/components/charts/VerticalBarBlock';

const PrototypeChartTest = () => {
  return (
    <PrototypePage wide>
      <LineChartBlock {...Data.AbstractChartProps} />
      <VerticalBarBlock {...Data.AbstractChartProps} />
    </PrototypePage>
  );
};

export default PrototypeChartTest;
