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

import { Axis } from '../../../../services/publicationService';
import { CharacteristicsData } from '../../../../services/tableBuilderService';
import { colours, parseCondensedYearRange, symbols } from './Charts';

interface StackedBarHorizontalProps {
  characteristicsData: CharacteristicsData;
  chartDataKeys: string[];
  labels: { [key: string]: string };
  xAxis: Axis;
  yAxis: Axis;
  height?: number;
}

export class StackedBarHorizontalBlock extends React.Component<
  StackedBarHorizontalProps
> {
  public render() {
    const chartData = this.props.characteristicsData.result.map(data => {
      return data.attributes;
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
          />
        ))}
      </BarChart>
    );
  }
}
