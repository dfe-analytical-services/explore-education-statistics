import ChartFunctions, {
  ChartProps,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import React from 'react';
import {
  Bar,
  BarChart,
  CartesianGrid,
  Legend,
  ResponsiveContainer,
  Tooltip,
} from 'recharts';

import { colours } from './Charts';

export default function VerticalBarBlock({
  data,
  indicators,
  height,
  xAxis,
  yAxis,
  width,
  meta,
}: ChartProps) {
  const chartData = data.result.map(({ measures, year }) => ({
    name: `${meta.timePeriods[year].label}`,
    ...measures,
  }));

  return (
    <ResponsiveContainer width={width || 900} height={height || 300}>
      <BarChart
        data={chartData}
        margin={ChartFunctions.calculateMargins(xAxis, yAxis, undefined)}
      >
        {ChartFunctions.calculateXAxis(xAxis, {
          interval: 0,
          tick: { fontSize: 12 },
          dataKey: xAxis.key || 'name',
        })}

        <CartesianGrid />

        {ChartFunctions.calculateYAxis(yAxis, { type: 'number' })}

        <Tooltip />
        <Legend />

        {indicators.map((dataKey, index) => {
          return (
            <Bar
              key={dataKey}
              dataKey={dataKey}
              fill={colours[index]}
              name={(meta && meta.indicators[dataKey].label) || 'a'}
              isAnimationActive={false}
            />
          );
        })}
      </BarChart>
    </ResponsiveContainer>
  );
}
