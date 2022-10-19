import { LineChartDataLabelPosition } from '@common/modules/charts/types/chart';
import { LegendInlinePosition } from '@common/modules/charts/types/legend';
import formatPretty from '@common/utils/number/formatPretty';
import { LabelProps } from 'recharts';
import React from 'react';

interface Props extends LabelProps {
  colour: string;
  dataLabelPosition?: LineChartDataLabelPosition;
  decimalPlaces?: number;
  index: number;
  name: string;
  legendLabelPosition: LegendInlinePosition;
  showDataLabels?: boolean;
  showLegendAsLabel?: boolean;
  totalDataPoints: number;
  unit?: string;
}

export default function LineChartLabel({
  colour,
  dataLabelPosition,
  decimalPlaces,
  index,
  legendLabelPosition,
  name,
  showDataLabels = false,
  showLegendAsLabel = false,
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
  if (showDataLabels) {
    return (
      <text
        dy={dataLabelPosition === 'above' ? '-10' : '20'}
        fill={colour}
        fontSize={14}
        textAnchor={getTextAnchor()}
        x={x}
        y={y}
      >
        <tspan>
          {value && formatPretty(value.toString(), unit, decimalPlaces)}
        </tspan>
      </text>
    );
  }

  // Legend as the label - only render for the last data point in the line.
  if (showLegendAsLabel && index === totalDataPoints - 1) {
    return (
      <text
        dy={legendLabelPosition === 'above' ? '-6' : '16'}
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
