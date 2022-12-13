import CustomReferenceLineLabel from '@common/modules/charts/components/CustomReferenceLineLabel';
import {
  Axis,
  AxisType,
  ReferenceLineStyle,
} from '@common/modules/charts/types/chart';
import { ChartData } from '@common/modules/charts/types/dataSet';
import React, { ReactElement } from 'react';
import { ReferenceLine, ReferenceLineProps } from 'recharts';
import { AxisDomainItem } from 'recharts/types/util/types';

interface Props
  extends Omit<ReferenceLineProps, 'label' | 'position' | 'style'> {
  axis: Axis;
  axisType: AxisType;
  chartData: ChartData[];
  label: string;
  otherAxisDomain?: [AxisDomainItem, AxisDomainItem];
  otherAxisPosition?: number;
  position: string | number;
  style?: ReferenceLineStyle;
}

export default function createReferenceLine({
  axis,
  chartData,
  label,
  otherAxisPosition,
  otherAxisDomain,
  axisType,
  position,
  style = 'dashed',
  ...props
}: Props): ReactElement {
  const styleProps = () => {
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

  return (
    <ReferenceLine
      {...styleProps()}
      {...props}
      key={`${position}_${label}`}
      position="middle"
      label={(lineProps: ReferenceLineProps) => (
        <CustomReferenceLineLabel
          viewBox={lineProps.viewBox}
          axis={axis}
          axisType={axisType}
          chartData={chartData}
          label={label}
          otherAxisDomain={otherAxisDomain}
          otherAxisPosition={otherAxisPosition}
          position={position}
        />
      )}
    />
  );
}
