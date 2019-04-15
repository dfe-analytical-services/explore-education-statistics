import React, { Component } from 'react';
import {
  Bar,
  BarChart,
  CartesianGrid,
  Legend,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';

import { ChartProps, colours } from './Charts';

interface StackedBarHorizontalProps extends ChartProps {
  stacked?: boolean;
}

export class HorizontalBarBlock extends Component<StackedBarHorizontalProps> {
  public render() {
    const chartData = this.props.characteristicsData.result.map(data => {
      return data.indicators;
    });

    return (
      <BarChart
        width={900}
        height={this.props.height || 600}
        data={chartData}
        layout="vertical"
        margin={{ top: 5, right: 30, left: 60, bottom: 25 }}
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
      </BarChart>
    );
  }
}
