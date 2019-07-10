import React from 'react';

import LineChartBlock from '@common/modules/find-statistics/components/charts/LineChartBlock';
import PrototypePage from '@admin/pages/prototypes/components/PrototypePage';

import Data from '@common/modules/find-statistics/components/charts/__tests__/__data__/testBlockData';
import VerticalBarBlock from '@common/modules/find-statistics/components/charts/VerticalBarBlock';
import HorizontalBarBlock from '@common/modules/find-statistics/components/charts/HorizontalBarBlock';
import MapBlock from '@common/modules/find-statistics/components/charts/MapBlock';

const PrototypeChartTest = () => {
  return (
    <PrototypePage wide>
      <VerticalBarBlock {...Data.AbstractMissingDataChartProps} />
    </PrototypePage>
  );
};

export default PrototypeChartTest;
