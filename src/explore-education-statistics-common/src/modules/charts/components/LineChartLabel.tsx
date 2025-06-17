import chunkLabelToFitCharLimit from '@common/modules/charts/components/utils/chunkLabelToFitCharLimit';
import { LineChartDataLabelPosition } from '@common/modules/charts/types/chart';
import {
  LegendInlinePosition,
  LegendLabelColour,
} from '@common/modules/charts/types/legend';
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
  labelColour?: LegendLabelColour;
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
  labelColour = 'inherit',
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
    const lineHeight = 16;

    let defaultDy = 0;
    if (position === 'below') {
      defaultDy = lineHeight;
    } else if (position === 'above') {
      defaultDy = -6;
    }
    const dy = defaultDy - inlinePositionOffset; // User can nudge lines up or down with this offset
    const isPositionRight = position === 'right';

    return (
      <text
        dy={dy}
        fill={labelColour === 'black' ? '#000' : colour}
        fontSize={14}
        textAnchor={isPositionRight ? 'start' : 'end'}
        x={x}
        y={y}
        data-testid="inline-legend-label"
      >
        {isPositionRight ? (
          // SVG <text> does not line wrap automatically :(
          // So wrap it manually into array of <tspan>s so it fits in chart margin
          chunkLabelToFitCharLimit(name).map((lineOfLabel, lineIndex) => (
            <tspan
              key={lineOfLabel}
              dx={30} // Shift right to account for xAxis padding
              dy={dy + lineIndex * lineHeight} // Shift each line down
              x={x}
              y={y}
            >
              {lineOfLabel}
            </tspan>
          ))
        ) : (
          <tspan>{name}</tspan>
        )}
      </text>
    );
  }
  return null;
}
