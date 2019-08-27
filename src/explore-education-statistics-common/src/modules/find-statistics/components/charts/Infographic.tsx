import {
  AbstractChartProps,
  ChartDefinition,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import * as React from 'react';

export interface InfographicChartProps extends AbstractChartProps {
  fileId?: string;
}

const Infographic = ({ fileId }: InfographicChartProps) => {
  return <div>Infographic ${fileId}</div>;
};

const definition: ChartDefinition = {
  type: 'infographic',
  name: 'Infographic',

  height: 600,

  capabilities: {
    dataSymbols: false,
    stackable: false,
    lineStyle: false,
    gridLines: false,
    canSize: true,
    fixedAxisGroupBy: true,
    hasAxes: false,
    hasReferenceLines: false,
  },

  data: [],

  axes: [],
};

Infographic.definition = definition;

export default Infographic;
