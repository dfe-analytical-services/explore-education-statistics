import { Axis, AxisType } from '@common/modules/charts/types/chart';
import { ChartData } from '@common/modules/charts/types/dataSet';
import getReferenceLineLabelPosition from '@common/modules/charts/components/utils/getReferenceLineLabelPosition';
import React, { memo } from 'react';
import parseNumber from '@common/utils/number/parseNumber';
import { AxisDomainItem, CartesianViewBox } from 'recharts/types/util/types';

interface Props {
  axis: Axis;
  axisType: AxisType;
  chartData: ChartData[];
  label: string;
  otherAxisDomain?: [AxisDomainItem, AxisDomainItem];
  otherAxisPosition?: number;
  position: string | number;
  viewBox?: CartesianViewBox;
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
  const otherAxisDomainMin = otherAxisDomain
    ? parseNumber(otherAxisDomain[0])
    : undefined;
  const otherAxisDomainMax = otherAxisDomain
    ? parseNumber(otherAxisDomain[1])
    : undefined;

  const labelPosition = getReferenceLineLabelPosition({
    axis,
    axisType,
    otherAxisDomainMin: axisType === 'major' ? otherAxisDomainMin : 0,
    otherAxisDomainMax: axisType === 'major' ? otherAxisDomainMax : 100, // otherAxisPosition is set as a percentage on minor axis lines
    otherAxisPosition,
    viewBox,
  });

  const getTextAnchor = () => {
    const isYAxisMinor = axisType === 'minor' && axis === 'y';
    const isYAxisMajor = axisType === 'major' && axis === 'y';

    if (
      position === chartData[0].name ||
      (isYAxisMinor && otherAxisPosition === 0) ||
      (isYAxisMajor && otherAxisPosition === otherAxisDomainMin)
    ) {
      return 'start';
    }

    if (
      position === chartData[chartData.length - 1].name ||
      (isYAxisMinor && otherAxisPosition === 100) ||
      (isYAxisMajor && otherAxisPosition === otherAxisDomainMax)
    ) {
      return 'end';
    }

    return 'middle';
  };

  return (
    <text
      className="govuk-!-font-size-16"
      dy={axis === 'y' ? -4 : 0}
      x={labelPosition.x}
      y={labelPosition.y}
      textAnchor={getTextAnchor()}
    >
      <tspan>{label}</tspan>
    </text>
  );
};

export default memo(CustomReferenceLineLabel);
