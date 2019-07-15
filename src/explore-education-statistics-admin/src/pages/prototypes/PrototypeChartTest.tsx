import React from 'react';

import LineChartBlock from '@common/modules/find-statistics/components/charts/LineChartBlock';
import PrototypePage from '@admin/pages/prototypes/components/PrototypePage';

import Data from '@common/modules/find-statistics/components/charts/__tests__/__data__/testBlockData';

const PrototypeChartTest = () => {
  return (
    <PrototypePage wide>
      <LineChartBlock {...Data.AbstractMissingDataChartProps} />
    </PrototypePage>
  );
};

export default PrototypeChartTest;
