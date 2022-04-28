import { ChartData } from '@common/modules/charts/types/dataSet';
import React, { memo } from 'react';
import { ViewBox } from 'recharts';

interface Props extends ViewBox {
  chartData: ChartData[];
  label: string;
  position: string | number;
}

const CustomReferenceLineLabel = ({
  chartData,
  label,
  position,
  height = 0,
  width = 0,
  x = 0,
  y = 0,
}: Props) => {
  const getTextAnchor = () => {
    if (position === chartData[0].name) {
      return 'start';
    }

    if (position === chartData[chartData.length - 1].name) {
      return 'end';
    }

    return 'middle';
  };

  return (
    <text
      className="govuk-!-font-size-16"
      x={width / 2 + x}
      y={height / 2 + y}
      textAnchor={getTextAnchor()}
    >
      <tspan>{label}</tspan>
    </text>
  );
};

export default memo(CustomReferenceLineLabel);
