import { LineChartDataLabelPosition } from '@common/modules/charts/types/chart';
import { LegendInlinePosition } from '@common/modules/charts/types/legend';
import formatPretty from '@common/utils/number/formatPretty';
import React from 'react';

interface Props {
  colour: string;
  decimalPlaces?: number;
  index: number;
  isDataLabel?: boolean;
  isLegendLabel?: boolean;
  name: string;
  position?: LineChartDataLabelPosition | LegendInlinePosition;
  totalDataPoints: number;
  unit?: string;
  value?: string | number;
  x?: string | number;
  y?: string | number;
}

export default function LineChartLabel({
  colour,
  decimalPlaces,
  index,
  isDataLabel = false,
  isLegendLabel = false,
  name,
  position,
  totalDataPoints,
  unit,
  value,
  x,
  y,
}: Props) {
  const getTextAnchor = () => {
    if (index === totalDataPoints - 1) {
      return 'end';
    }
    if (index === 0) {
      return 'start';
    }
    return 'middle';
  };

  // Labels on each data point
  if (isDataLabel) {
    return (
      <text
        dy={position === 'above' ? '-10' : '20'}
        fill={colour}
        fontSize={14}
        textAnchor={getTextAnchor()}
        x={x}
        y={y}
      >
        <tspan>
          {typeof value !== 'undefined' &&
            formatPretty(value, unit, decimalPlaces)}
        </tspan>
      </text>
    );
  }

  // Legend as the label - only render for the last data point in the line.
  if (isLegendLabel && index === totalDataPoints - 1) {
    return (
      <text
        dy={position === 'above' ? '-6' : '16'}
        fill={colour}
        fontSize={14}
        textAnchor="end"
        x={x}
        y={y}
      >
        <tspan>{name}</tspan>
      </text>
    );
  }
  return null;
}
