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

interface StackedBarHorizontalProps {
  nothing?: string;
}

export class StackedBarHorizontalBlock extends React.Component<
  StackedBarHorizontalProps
> {
  public render() {
    // tslint:disable
    const chartData = [
      { name: 'SEN', ebacc_entry: 12, eng: 13, attainment: 27 },
      { name: 'No SEN', ebacc_entry: 12, eng: 13, attainment: 27 },

      { name: '' },

      { name: 'Disadvantage', ebacc_entry: 12, eng: 13, attainment: 27 },
      { name: 'All other pupils', ebacc_entry: 12, eng: 13, attainment: 27 },

      { name: '' },

      { name: 'Boys', ebacc_entry: 12, eng: 13, attainment: 27 },
      { name: 'Girls', ebacc_entry: 12, eng: 13, attainment: 27 },
    ];

    const chartDataKeys = ['ebacc_entry', 'eng', 'attainment'];
    const chartLabels: any = {
      ebacc_entry: 'Ebacc Entry',
      eng: 'Eng & Maths (9-5)',
      attainment: 'Attainment 8',
    };

    return (
      <BarChart
        width={900}
        height={600}
        data={chartData}
        layout="vertical"
        margin={{ top: 5, right: 30, left: 60, bottom: 25 }}
      >
        <YAxis type="category" dataKey="name" />
        <CartesianGrid />
        <XAxis type="number" />
        <Tooltip cursor={false} />
        <Legend />

        {chartDataKeys.map((key, index) => (
          <Bar
            key={index}
            dataKey={key}
            name={chartLabels[key]}
            fill={colours[index]}
          />
        ))}
      </BarChart>
    );
  }
}
