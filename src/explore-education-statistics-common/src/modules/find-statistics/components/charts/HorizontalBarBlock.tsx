import {
  calculateXAxis,
  calculateYAxis,
  generateReferenceLines,
  ChartDefinition,
  ChartProps,
  ChartDataB,
  createDataForAxis,
  getKeysForChart,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import React, { Component } from 'react';
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

export default class HorizontalBarBlock extends Component<
  StackedBarHorizontalProps
> {
  public static definition: ChartDefinition = {
    type: 'horizontalbar',
    name: 'Horizontal Bar',

    data: [
      {
        type: 'bar',
        title: 'Bar',
        entryCount: 1,
        targetAxis: 'yaxis',
      },
    ],

    axes: [
      {
        id: 'yaxis',
        title: 'Y Axis',
        type: 'major',
      },
    ],
  };

  public render() {
    const {
      data,
      height,
      width,
      referenceLines,
      stacked,
      dataLabels,
      axes,
    } = this.props;

    const chartData: ChartDataB[] = createDataForAxis(axes.major, data.result);

    const keysForChart = getKeysForChart(chartData);

    const yAxis = { key: undefined, title: '' };
    const xAxis = { key: undefined, title: '' };

    return (
      <ResponsiveContainer width={width || '100%'} height={height || 600}>
        <BarChart data={chartData} layout="vertical">
          {calculateYAxis(yAxis, {
            type: 'category',
            dataKey: (xAxis.key || ['name'])[0],
          })}

          <CartesianGrid />

          {calculateXAxis(xAxis, { type: 'number' })}

          <Tooltip cursor={false} />
          <Legend />

          {Array.from(keysForChart).map((dataKey, index) => (
            <Bar
              key={dataKey}
              dataKey={dataKey}
              fill={colours[index]}
              name={dataLabels[dataKey] && dataLabels[dataKey].label}
              unit={dataLabels[dataKey] && dataLabels[dataKey].unit}
              stackId={stacked ? 'a' : undefined}
            />
          ))}

          {referenceLines && generateReferenceLines(referenceLines)}
        </BarChart>
      </ResponsiveContainer>
    );
  }
}
