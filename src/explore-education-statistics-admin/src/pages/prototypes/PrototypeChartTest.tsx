/* eslint-disable @typescript-eslint/no-unused-vars */
import ChartBuilder from '@admin/modules/chart-builder/ChartBuilder';
import React from 'react';

import PrototypePage from '@admin/pages/prototypes/components/PrototypePage';

import PrototypeData from '@admin/pages/prototypes/PrototypeData';
import { DataBlockResponse } from '@common/services/dataBlockService';

const PrototypeChartTest = () => {
  const [data] = React.useState<DataBlockResponse>(PrototypeData.testResponse);

  return (
    <PrototypePage wide>
      <ChartBuilder data={data} />
    </PrototypePage>
  );
};

export default PrototypeChartTest;
