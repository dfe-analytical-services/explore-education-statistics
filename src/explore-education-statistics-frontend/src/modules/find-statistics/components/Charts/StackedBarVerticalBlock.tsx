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

interface StackedBarVerticalProps {
  characteristicsData: CharacteristicsData;
  chartDataKeys: string[];
  labels: { [key: string]: string };
  xAxis: Axis;
  yAxis: Axis;
  height?: number;
}

export class StackedBarVerticalBlock extends React.Component<
  StackedBarVerticalProps
> {
  public render() {
    // tslint:disable

    const chartData = this.props.characteristicsData.result.map(data => {
      return data.indicators;
    });

    return (
      <BarChart
        width={900}
        height={this.props.height || 300}
        data={chartData}
        margin={{ top: 5, right: 30, left: 20, bottom: 25 }}
      >
        <XAxis
          dataKey={this.props.xAxis.key || 'name'}
          interval={0}
          tick={{ fontSize: 12 }}
        />
        <CartesianGrid />
        <YAxis />
        <Tooltip />
        <Legend />

        {this.props.chartDataKeys.map((key, index) => (
          <Bar
            key={index}
            dataKey={key}
            fill={colours[index]}
            name={this.props.labels[key]}
          />
        ))}
      </BarChart>
    );
  }
}
