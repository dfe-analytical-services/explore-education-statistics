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

import { colours, parseCondensedYearRange, symbols } from './Charts';

interface StackedBarVerticalProps {
  nothing?: string;
}

export class StackedBarVerticalBlock extends React.Component<
  StackedBarVerticalProps
> {
  public render() {
    // tslint:disable
    const chartData = [
      { name: 'London', floor_standards: 5, coasting: 3.5 },
      { name: 'Yorkshire', floor_standards: 7, coasting: 6 },
      { name: 'East', floor_standards: 8, coasting: 5 },
      { name: 'South East', floor_standards: 11, coasting: 9 },
      { name: 'South West', floor_standards: 11, coasting: 11 },
      { name: 'West Midlands', floor_standards: 12, coasting: 7.5 },
      { name: 'East Midlands', floor_standards: 14, coasting: 10 },
      { name: 'North West', floor_standards: 21, coasting: 12 },
      { name: 'North East', floor_standards: 23, coasting: 11 },
    ];

    const chartDataKeys = ['floor_standards', 'coasting'];

    return (
      <BarChart
        width={900}
        height={300}
        data={chartData}
        margin={{ top: 5, right: 30, left: 20, bottom: 25 }}
      >
        <XAxis dataKey="name" />
        <CartesianGrid />
        <YAxis />
        <Tooltip />
        <Legend />

        {chartDataKeys.map((key, index) => (
          <Bar key={index} dataKey={key} fill={colours[index]} />
        ))}
      </BarChart>
    );
  }
}
