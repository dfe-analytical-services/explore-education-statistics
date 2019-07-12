import React from 'react';

import LineChartBlock from '@common/modules/find-statistics/components/charts/LineChartBlock';
import PrototypePage from '@admin/pages/prototypes/components/PrototypePage';

import PrototypeData from '@common/modules/find-statistics/components/charts/__tests__/__data__/testBlockData';
import VerticalBarBlock from '@common/modules/find-statistics/components/charts/VerticalBarBlock';
import HorizontalBarBlock from '@common/modules/find-statistics/components/charts/HorizontalBarBlock';
import MapBlock from '@common/modules/find-statistics/components/charts/MapBlock';
import PrototypeData2 from '@admin/pages/prototypes/PrototypeData';
import ChartBuilder from '@admin/modules/chart-builder/ChartBuilder';
import { DataBlockResponse } from '@common/services/dataBlockService';

const PrototypeChartTest = () => {
  const [data] = React.useState<DataBlockResponse>(PrototypeData2.testResponse);

  return (
    <PrototypePage wide>{data && <ChartBuilder data={data} />}</PrototypePage>
  );
};

export default PrototypeChartTest;
