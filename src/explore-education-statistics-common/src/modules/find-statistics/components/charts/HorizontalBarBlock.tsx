import React, { Component } from 'react';
import {
  Bar,
  BarChart,
  CartesianGrid,
  Legend,
  ReferenceLine,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';

import * as PublicationService from '@common/services/publicationService';

import { ChartProps, colours } from './Charts';

interface StackedBarHorizontalProps extends ChartProps {
  stacked?: boolean;
}

export class HorizontalBarBlock extends Component<StackedBarHorizontalProps> {
  public calculateMargins() {
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

  public generateReferenceLines() {
    const generateReferenceLine = (
      line: PublicationService.ReferenceLine,
      idx: number,
    ) => {
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
      return <ReferenceLine {...referenceLineProps} />;
    };

    if (this.props.referenceLines && this.props.referenceLines.length > 0) {
      return this.props.referenceLines.map(generateReferenceLine);
    }

    return '';
  }

  public render() {
    const chartData = this.props.characteristicsData.result.map(data => {
      return data.indicators;
    });

    return (
      <ResponsiveContainer width={900} height={this.props.height || 600}>
        <BarChart
          data={chartData}
          layout="vertical"
          margin={this.calculateMargins()}
        >
          <YAxis type="category" dataKey={this.props.yAxis.key || 'name'} />
          <CartesianGrid />
          <XAxis type="number" />
          <Tooltip cursor={false} />
          <Legend />

          {this.props.chartDataKeys.map((key, index) => (
            <Bar
              key={index}
              dataKey={key}
              name={this.props.labels[key]}
              fill={colours[index]}
              stackId={this.props.stacked ? 'a' : undefined}
            />
          ))}

          {this.generateReferenceLines()}
        </BarChart>
      </ResponsiveContainer>
    );
  }
}
