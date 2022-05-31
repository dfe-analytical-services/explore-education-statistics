import CustomReferenceLineLabel from '@common/modules/charts/components/CustomReferenceLineLabel';
import { ChartData } from '@common/modules/charts/types/dataSet';
import React, { ReactElement } from 'react';
import { ReferenceLine, ReferenceLineProps } from 'recharts';

interface Props extends Omit<ReferenceLineProps, 'label' | 'position'> {
  chartData: ChartData[];
  label: string;
  position: string | number;
}

export default function createReferenceLine({
  chartData,
  label,
  position,
  ...props
}: Props): ReactElement {
  return (
    <ReferenceLine
      key={`${position}_${label}`}
      stroke="#b1b4b6"
      strokeDasharray="8 4"
      strokeWidth={3}
      {...props}
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
