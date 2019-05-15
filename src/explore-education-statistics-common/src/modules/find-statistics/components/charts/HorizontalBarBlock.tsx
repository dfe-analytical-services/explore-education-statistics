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

interface StackedBarHorizontalProps extends ChartProps {
  stacked?: boolean;
}

export default function HorizontalBarBlock(props: StackedBarHorizontalProps) {
  const {
    data,
    meta,
    height,
    width,
    xAxis,
    referenceLines,
    yAxis,
    stacked,
    indicators,
  } = props;

  const chartData = data.result.map(({ measures, year }) => ({
    name: `${meta.timePeriods[year].label}`,
    ...measures,
  }));

  return (
    <ResponsiveContainer width={width || '100%'} height={height || 600}>
      <BarChart
        data={chartData}
        layout="vertical"
        margin={ChartFunctions.calculateMargins(yAxis, xAxis, referenceLines)}
      >
        {ChartFunctions.calculateYAxis(yAxis, {
          type: 'category',
          dataKey: yAxis.key || 'name',
        })}

        <CartesianGrid />

        {ChartFunctions.calculateXAxis(xAxis, { type: 'number' })}

        <Tooltip cursor={false} />
        <Legend />

        {indicators.map((dataKey, index) => {
          return (
            <Bar
              key={dataKey}
              dataKey={dataKey}
              name={(meta && meta.indicators[dataKey].label) || 'a'}
              fill={colours[index]}
              stackId={stacked ? 'a' : undefined}
              isAnimationActive={false}
            />
          );
        })}

        {referenceLines &&
          ChartFunctions.generateReferenceLines(referenceLines)}
      </BarChart>
    </ResponsiveContainer>
  );
}
