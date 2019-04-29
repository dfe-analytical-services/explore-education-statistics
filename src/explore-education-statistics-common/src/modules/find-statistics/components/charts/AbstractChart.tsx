import { Axis, ReferenceLine } from '@common/services/publicationService';
import { CharacteristicsData } from '@common/services/tableBuilderService';
import React, { Component, ReactNode } from 'react';
import {
  Label,
  PositionType,
  ReferenceLine as RechartsReferenceLine,
  XAxis,
  XAxisProps,
  YAxis,
  YAxisProps,
} from 'recharts';

export interface ChartProps {
  characteristicsData: CharacteristicsData;
  chartDataKeys: string[];
  labels: { [key: string]: string };
  xAxis: Axis;
  yAxis: Axis;
  height?: number;
  referenceLines?: ReferenceLine[];
}

export class AbstractChart<P extends ChartProps, S = {}> extends Component<
  P,
  S
> {
  private static calculateAxis(
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
  }

  protected calculateMargins() {
    const margin = {
      top: 15,
      right: 30,
      left: 60,
      bottom: 25,
    };

    const { xAxis, referenceLines } = this.props;

    if (referenceLines && referenceLines.length > 0) {
      referenceLines.forEach(line => {
        if (line.x) margin.top = 25;
        if (line.y) margin.left = 75;
      });
    }

    if (xAxis.title) {
      // margin.bottom +=25;
    }

    return margin;
  }

  protected calculateXAxis(props: XAxisProps): ReactNode {
    const { xAxis } = this.props;
    const { size: height, title } = AbstractChart.calculateAxis(
      xAxis,
      'insideBottom',
    );
    return (
      <XAxis {...props} height={height}>
        {title}
      </XAxis>
    );
  }

  protected calculateYAxis(props: YAxisProps): ReactNode {
    const { yAxis } = this.props;
    const { size: width, title } = AbstractChart.calculateAxis(
      yAxis,
      'left',
      270,
      90,
    );
    return (
      <YAxis {...props} width={width}>
        {title}
      </YAxis>
    );
  }

  protected generateReferenceLines() {
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

    const { referenceLines } = this.props;

    if (referenceLines && referenceLines.length > 0) {
      return referenceLines.map(generateReferenceLine);
    }

    return '';
  }
}
