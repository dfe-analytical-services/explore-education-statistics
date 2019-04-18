import { CharacteristicsData } from '@common/services/tableBuilderService';
import { Axis, ReferenceLine } from '@common/services/publicationService';
import { Component } from 'react';
import React from 'react';
import * as Recharts from 'recharts';

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
  protected calculateMargins() {
    const margin = {
      top: 15,
      right: 30,
      left: 60,
      bottom: 25,
    };

    if (this.props.referenceLines && this.props.referenceLines.length > 0) {
      this.props.referenceLines.forEach(line => {
        if (line.x) margin.top = 25;
        if (line.y) margin.left = 75;
      });
    }

    return margin;
  }

  protected generateReferenceLines() {
    const generateReferenceLine = (line: ReferenceLine, idx: number) => {
      const referenceLineProps = {
        key: `ref_${idx}`,
        ...line,
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
      return <Recharts.ReferenceLine {...referenceLineProps} />;
    };

    if (this.props.referenceLines && this.props.referenceLines.length > 0) {
      return this.props.referenceLines.map(generateReferenceLine);
    }

    return '';
  }
}
