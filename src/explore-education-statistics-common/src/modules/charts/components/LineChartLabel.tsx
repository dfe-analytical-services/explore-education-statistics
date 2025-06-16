import { LineChartDataLabelPosition } from '@common/modules/charts/types/chart';
import { LegendInlinePosition } from '@common/modules/charts/types/legend';
import formatPretty from '@common/utils/number/formatPretty';
import React from 'react';

interface Props {
  colour: string;
  decimalPlaces?: number;
  index: number;
  inlinePositionOffset?: number;
  isDataLabel?: boolean;
  isLegendLabel?: boolean;
  isLastItem?: boolean;
  name: string;
  position?: LineChartDataLabelPosition | LegendInlinePosition;
  unit?: string;
  value?: string | number;
  x?: string | number;
  y?: string | number;
}

export default function LineChartLabel({
  colour,
  decimalPlaces,
  index,
  inlinePositionOffset = 0,
  isDataLabel = false,
  isLegendLabel = false,
  isLastItem = false,
  name,
  position,
  unit,
  value,
  x,
  y,
}: Props) {
  const getTextAnchor = () => {
    if (isLastItem) {
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
  if (isLegendLabel && isLastItem) {
    const defaultDy = position === 'below' ? 16 : -6;

    return (
      <text
        dy={defaultDy + inlinePositionOffset * -1}
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
