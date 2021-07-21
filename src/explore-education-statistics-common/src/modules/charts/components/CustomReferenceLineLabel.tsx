import { ReferenceLine } from '@common/modules/charts/types/chart';
import { ChartData } from '@common/modules/charts/types/dataSet';
import React from 'react';
import { ReferenceLineProps } from 'recharts';

interface Props {
  chartData: ChartData[];
  referenceLine: ReferenceLine;
  referenceLineProps: ReferenceLineProps;
}

const CustomReferenceLineLabel = ({
  chartData,
  referenceLine,
  referenceLineProps,
}: Props) => {
  const getTextAnchor = () => {
    if (referenceLine.position === chartData[0].name) {
      return 'start';
    }
    if (referenceLine.position === chartData[chartData.length - 1].name) {
      return 'end';
    }
    return 'middle';
  };

  const getY = () => {
    const y = referenceLineProps.viewBox?.y ?? 0;
    const height = referenceLineProps.viewBox?.height ?? 0;
    return height / 2 + y;
  };

  return (
    <text
      x={referenceLineProps.viewBox?.x}
      y={getY()}
      textAnchor={getTextAnchor()}
    >
      <tspan>{referenceLine.label}</tspan>
    </text>
  );
};

export default CustomReferenceLineLabel;
