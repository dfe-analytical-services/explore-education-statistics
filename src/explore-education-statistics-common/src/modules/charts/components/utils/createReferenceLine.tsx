import CustomReferenceLineLabel from '@common/modules/charts/components/CustomReferenceLineLabel';
import {
  Axis,
  AxisType,
  ReferenceLineStyle,
} from '@common/modules/charts/types/chart';
import { ChartData } from '@common/modules/charts/types/dataSet';
import parseNumber from '@common/utils/number/parseNumber';
import React, { ReactElement } from 'react';
import { ReferenceLine, ReferenceLineProps } from 'recharts';
import { AxisDomainItem } from 'recharts/types/util/types';

interface Props {
  axis: Axis;
  axisDomain?: [AxisDomainItem, AxisDomainItem];
  axisType: AxisType;
  chartBottomMargin?: number;
  chartData: ChartData[];
  chartInnerHeight?: number;
  label: string;
  labelWidth?: number;
  otherAxisDomain?: [AxisDomainItem, AxisDomainItem];
  otherAxisEnd?: string;
  otherAxisPosition?: number;
  otherAxisStart?: string;
  perpendicularLine?: boolean;
  position?: string | number;
  style?: ReferenceLineStyle;
  x?: string | number;
  y?: string | number;
}

export default function createReferenceLine({
  axis,
  axisDomain,
  chartBottomMargin,
  chartData,
  chartInnerHeight,
  label,
  labelWidth,
  otherAxisDomain,
  otherAxisEnd,
  otherAxisPosition,
  otherAxisStart,
  axisType,
  perpendicularLine,
  position,
  style = 'dashed',
  x,
  y,
}: Props): ReactElement {
  const getStyleProps = () => {
    switch (style) {
      case 'dashed':
        return {
          stroke: '#b1b4b6',
          strokeDasharray: '8 4',
          strokeWidth: 3,
        };
      case 'none':
        return {
          stroke: 'transparent',
          strokeWidth: 0,
        };
      case 'solid':
        return {
          stroke: '#b1b4b6',
          strokeWidth: 3,
        };
      default:
        return {};
    }
  };

  const styleProps = getStyleProps();

  return (
    <ReferenceLine
      // Hide the reference line for perpendicular lines as we draw them
      // manually with the label.
      {...(perpendicularLine ? { stroke: 'none' } : styleProps)}
      key={`${position}_${label}`}
      x={otherAxisStart && otherAxisEnd ? undefined : x}
      y={otherAxisStart && otherAxisEnd ? undefined : y}
      segment={getSegment({
        axis,
        axisDomain,
        otherAxisEnd,
        position,
        otherAxisStart,
      })}
      label={(lineProps: ReferenceLineProps) => (
        <CustomReferenceLineLabel
          viewBox={lineProps.viewBox}
          axis={axis}
          axisType={axisType}
          chartBottomMargin={chartBottomMargin}
          chartData={chartData}
          chartInnerHeight={chartInnerHeight}
          label={label}
          labelWidth={labelWidth}
          otherAxisDomain={otherAxisDomain}
          otherAxisPosition={otherAxisPosition}
          perpendicularLine={perpendicularLine}
          position={position}
          styleProps={styleProps}
        />
      )}
    />
  );
}

/**
 * Segments are used to draw a reference line between two points.
 */
function getSegment({
  axis,
  axisDomain,
  otherAxisEnd,
  position,
  otherAxisStart,
}: {
  axis: Axis;
  axisDomain?: [AxisDomainItem, AxisDomainItem];
  otherAxisEnd?: string;
  position?: string | number;
  otherAxisStart?: string;
}) {
  if (!otherAxisStart || !otherAxisEnd || !position) {
    return undefined;
  }
  const axisDomainMin = parseNumber(axisDomain?.[0]);
  const axisDomainMax = parseNumber(axisDomain?.[1]);

  const otherAxisDefaultPosition =
    axisDomainMax !== undefined && axisDomainMin !== undefined
      ? (axisDomainMax - axisDomainMin) / 2
      : undefined;

  return axis === 'x'
    ? [
        {
          x: position ?? otherAxisDefaultPosition,
          y: otherAxisStart,
        },
        { x: position ?? otherAxisDefaultPosition, y: otherAxisEnd },
      ]
    : [
        {
          x: otherAxisStart,
          y: position ?? otherAxisDefaultPosition,
        },
        { x: otherAxisEnd, y: position ?? otherAxisDefaultPosition },
      ];
}
