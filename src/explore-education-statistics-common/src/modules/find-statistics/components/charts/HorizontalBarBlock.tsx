import {
  calculateXAxis,
  calculateYAxis,
  generateReferenceLines,
  ChartDefinition,
  ChartDataB,
  createDataForAxis,
  getKeysForChart,
  mapNameToNameLabel,
  StackedBarProps,
  populateDefaultChartProps,
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
import LoadingSpinner from '@common/components/LoadingSpinner';

export default class HorizontalBarBlock extends Component<StackedBarProps> {
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
        defaultDataType: 'timePeriods',
      },
    ],
  };

  public render() {
    const {
      data,
      meta,
      height,
      width,
      referenceLines,
      stacked = false,
      labels,
      axes,
    } = this.props;

    if (!axes.major || !data) return <LoadingSpinner />;

    const chartData: ChartDataB[] = createDataForAxis(
      axes.major,
      data.result,
      meta,
    ).map(mapNameToNameLabel(labels));

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

          {Array.from(keysForChart).map(name => (
            <Bar
              key={name}
              {...populateDefaultChartProps(name, labels[name])}
              stackId={stacked ? 'a' : undefined}
            />
          ))}

          {referenceLines && generateReferenceLines(referenceLines)}
        </BarChart>
      </ResponsiveContainer>
    );
  }
}
