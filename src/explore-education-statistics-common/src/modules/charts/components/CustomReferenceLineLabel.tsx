import { Axis, AxisType } from '@common/modules/charts/types/chart';
import { ChartData } from '@common/modules/charts/types/dataSet';
import styles from '@common/modules/charts/components/CustomReferenceLineLabel.module.scss';
import getReferenceLineLabelPosition from '@common/modules/charts/components/utils/getReferenceLineLabelPosition';
import React, { memo } from 'react';
import parseNumber from '@common/utils/number/parseNumber';
import { AxisDomainItem, CartesianViewBox } from 'recharts/types/util/types';
import { Text } from 'recharts';

interface StyleProps {
  stroke?: string;
  strokeDasharray?: string;
  strokeWidth?: number;
}

interface Props {
  axis: Axis;
  axisType: AxisType;
  chartBottomMargin?: number;
  chartData: ChartData[];
  chartInnerHeight?: number;
  label: string;
  labelWidth?: number;
  otherAxisDomain?: [AxisDomainItem, AxisDomainItem];
  otherAxisPosition?: number;
  perpendicularLine?: boolean;
  position?: string | number;
  styleProps?: StyleProps;
  viewBox?: CartesianViewBox;
}

const CustomReferenceLineLabel = ({
  axis,
  axisType,
  chartBottomMargin = 0,
  chartData,
  chartInnerHeight,
  label,
  labelWidth,
  otherAxisDomain,
  otherAxisPosition,
  perpendicularLine = false,
  position,
  styleProps,
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
    perpendicularLine,
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
    <>
      {/* Manually draw a line when it's perpendicular to the axis
      (added between data points on the major axis) */}
      {perpendicularLine && (
        <PerpendicularLine
          chartBottomMargin={chartBottomMargin}
          chartInnerHeight={chartInnerHeight}
          styleProps={styleProps}
          viewBox={viewBox}
        />
      )}
      <Text
        className={styles.text}
        dy={axis === 'y' ? -4 : 0}
        x={labelPosition.x}
        y={labelPosition.y}
        textAnchor={getTextAnchor()}
        width={labelWidth}
      >
        {label}
      </Text>
    </>
  );
};

export default memo(CustomReferenceLineLabel);

function PerpendicularLine({
  chartBottomMargin,
  chartInnerHeight,
  styleProps,
  viewBox,
}: {
  chartBottomMargin?: number;
  chartInnerHeight?: number;
  styleProps?: StyleProps;
  viewBox?: CartesianViewBox;
}) {
  if (!viewBox || !viewBox.x || !viewBox.width) {
    return null;
  }
  const x = viewBox.x + viewBox.width / 2;

  return (
    <line
      // eslint-disable-next-line react/jsx-props-no-spreading
      {...styleProps}
      x1={x}
      y1={chartInnerHeight}
      x2={x}
      y2={chartBottomMargin}
      className="recharts-reference-line-line"
    />
  );
}
