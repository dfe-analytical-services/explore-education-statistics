import { Axis, ReferenceLine } from '@common/services/publicationService';
import React, { ReactNode } from 'react';
import {
  Label,
  PositionType,
  ReferenceLine as RechartsReferenceLine,
  XAxis,
  XAxisProps,
  YAxis,
  YAxisProps,
} from 'recharts';
import {
  DataBlockData,
  DataBlockMetadata,
} from '@common/services/dataBlockService';

export interface ChartProps {
  data: DataBlockData;
  meta: DataBlockMetadata;
  chartDataKeys: string[];
  labels: { [key: string]: string };
  xAxis: Axis;
  yAxis: Axis;
  height?: number;
  width?: number;
  referenceLines?: ReferenceLine[];
}

const ChartFunctions = {
  calculateAxis(
    axis: Axis,
    position: PositionType,
    angle: number = 0,
    titleSize: number = 25,
  ) {
    let size = axis.size || 25;
    let title: ReactNode | '';

    if (axis.title) {
      size += titleSize;
      title = (
        <Label position={position} angle={angle}>
          {axis.title}
        </Label>
      );
    }

    return { size, title };
  },

  calculateMargins(xAxis: Axis, yAxis: Axis, referenceLines?: ReferenceLine[]) {
    const margin = {
      top: 15,
      right: 30,
      left: 60,
      bottom: 25,
    };

    if (referenceLines && referenceLines.length > 0) {
      referenceLines.forEach(line => {
        if (line.x) margin.top = 25;
        if (line.y) margin.left = 75;
      });
    }

    if (xAxis.title) {
      margin.bottom += 25;
    }

    return margin;
  },

  calculateXAxis(xAxis: Axis, axisProps: XAxisProps): ReactNode {
    const { size: height, title } = ChartFunctions.calculateAxis(
      xAxis,
      'insideBottom',
    );
    return (
      <XAxis {...axisProps} height={height}>
        {title}
      </XAxis>
    );
  },

  calculateYAxis(yAxis: Axis, axisProps: YAxisProps): ReactNode {
    const { size: width, title } = ChartFunctions.calculateAxis(
      yAxis,
      'left',
      270,
      90,
    );
    return (
      <YAxis {...axisProps} width={width}>
        {title}
      </YAxis>
    );
  },

  generateReferenceLines(referenceLines: ReferenceLine[]): ReactNode {
    const generateReferenceLine = (line: ReferenceLine, idx: number) => {
      const referenceLineProps = {
        key: `ref_${idx}`,
        ...line,
        stroke: 'black',
        strokeWidth: '2px',

        label: {
          position: 'top',
          value: line.label,
        },
      };

      // Using <Label> in the label property is causing an infinite loop
      // forcing the use of the properties directly as per https://github.com/recharts/recharts/issues/730
      // appears to be a fix, but this is not valid for the types.
      // issue raised https://github.com/recharts/recharts/issues/1710
      // @ts-ignore
      return <RechartsReferenceLine {...referenceLineProps} />;
    };

    return referenceLines.map(generateReferenceLine);
  },
};

export default ChartFunctions;
