import CustomReferenceLineLabel from '@common/modules/charts/components/CustomReferenceLineLabel';
import { ReferenceLineStyle } from '@common/modules/charts/types/chart';
import { ChartData } from '@common/modules/charts/types/dataSet';
import React, { ReactElement } from 'react';
import { ReferenceLine, ReferenceLineProps } from 'recharts';

interface Props
  extends Omit<ReferenceLineProps, 'label' | 'position' | 'style'> {
  chartData: ChartData[];
  label: string;
  position: string | number;
  style?: ReferenceLineStyle;
}

export default function createReferenceLine({
  chartData,
  label,
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
      default:
        return {};
    }
  };

  return (
    <ReferenceLine
      {...styleProps()}
      {...props}
      key={`${position}_${label}`}
      label={(lineProps: ReferenceLineProps) => (
        <CustomReferenceLineLabel
          {...lineProps.viewBox}
          chartData={chartData}
          label={label}
          position={position}
        />
      )}
    />
  );
}
