import { LegendInlinePosition } from '@common/modules/charts/types/legend';
import { LabelProps } from 'recharts';
import React from 'react';

interface Props extends LabelProps {
  colour: string;
  index: number;
  name: string;
  labelPosition: LegendInlinePosition;
  totalDataPoints: number;
}

export default function LineChartLegendLabel({
  colour,
  index,
  labelPosition,
  name,
  totalDataPoints,
  x,
  y,
}: Props) {
  // Only render a label for the last data point in the line.
  if (index === totalDataPoints - 1) {
    return (
      <text
        x={x}
        y={y}
        dy={labelPosition === 'above' ? '-6' : '16'}
        fill={colour}
        fontSize={14}
        textAnchor="end"
      >
        <tspan>{name}</tspan>
      </text>
    );
  }
  return null;
}
