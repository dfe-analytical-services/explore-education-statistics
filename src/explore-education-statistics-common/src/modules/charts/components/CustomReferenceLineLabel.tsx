import { Axis, AxisType } from '@common/modules/charts/types/chart';
import { ChartData } from '@common/modules/charts/types/dataSet';
import getReferenceLineLabelPosition from '@common/modules/charts/components/utils/getReferenceLineLabelPosition';
import React, { memo } from 'react';
import { AxisDomain, ViewBox } from 'recharts';

interface Props {
  axis: Axis;
  axisType: AxisType;
  chartData: ChartData[];
  label: string;
  otherAxisDomain?: [AxisDomain, AxisDomain];
  otherAxisPosition?: number;
  position: string | number;
  viewBox?: ViewBox;
}

const CustomReferenceLineLabel = ({
  axis,
  axisType,
  chartData,
  label,
  otherAxisDomain,
  otherAxisPosition,
  position,
  viewBox,
}: Props) => {
  const labelPosition = getReferenceLineLabelPosition({
    axis,
    axisType,
    otherAxisDomain,
    otherAxisPosition,
    viewBox,
  });

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
      dy={0}
      x={labelPosition.xPosition}
      y={labelPosition.yPosition}
      textAnchor={getTextAnchor()}
    >
      <tspan>{label}</tspan>
    </text>
  );
};

export default memo(CustomReferenceLineLabel);
