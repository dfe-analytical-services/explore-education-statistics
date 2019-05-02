import { ChartProps } from '@common/modules/find-statistics/components/charts/AbstractChart';
import React from 'react';
import {
  Bar,
  BarChart,
  CartesianGrid,
  Legend,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';

import { colours } from './Charts';

export default function VerticalBarBlock({
  data,
  chartDataKeys,
  height,
  labels,
  xAxis,
}: ChartProps) {
  const chartData = data.result.map(dataItem => {
    return dataItem.measures;
  });

  return (
    <ResponsiveContainer width={900} height={height || 300}>
      <BarChart
        data={chartData}
        margin={{ top: 5, right: 30, left: 20, bottom: 25 }}
      >
        <XAxis
          dataKey={xAxis.key || 'name'}
          interval={0}
          tick={{ fontSize: 12 }}
        />
        <CartesianGrid />
        <YAxis />
        <Tooltip />
        <Legend />

        {chartDataKeys.map((dataKey, index) => {
          const key = index;
          return (
            <Bar
              key={key}
              dataKey={dataKey}
              fill={colours[index]}
              name={labels[dataKey]}
            />
          );
        })}
      </BarChart>
    </ResponsiveContainer>
  );
}
