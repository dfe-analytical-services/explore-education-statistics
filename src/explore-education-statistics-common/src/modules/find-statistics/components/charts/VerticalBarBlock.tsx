import {
  calculateMargins,
  calculateXAxis,
  calculateYAxis,
  ChartDataB,
  ChartDefinition,
  createDataForAxis,
  getKeysForChart,
  mapNameToNameLabel,
  populateDefaultChartProps,
  StackedBarProps,
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

export default class VerticalBarBlock extends Component<StackedBarProps> {
  public static definition: ChartDefinition = {
    type: 'verticalbar',
    name: 'Vertical Bar',

    data: [
      {
        type: 'bar',
        title: 'Bar',
        entryCount: 1,
        targetAxis: 'xaxis',
      },
    ],

    axes: [
      {
        id: 'xaxis',
        title: 'X Axis',
        type: 'major',
        defaultDataType: 'timePeriod',
      },
    ],
  };

  public render() {
    const { data, height, width, labels, axes, stacked } = this.props;

    if (!axes.major || !data) return <LoadingSpinner />;

    const chartData: ChartDataB[] = createDataForAxis(
      axes.major,
      data.result,
    ).map(mapNameToNameLabel(labels));

    const keysForChart = getKeysForChart(chartData);

    const yAxis = { key: undefined, title: '' };
    const xAxis = { key: undefined, title: '' };

    return (
      <ResponsiveContainer width={width || 900} height={height || 300}>
        <BarChart
          data={chartData}
          margin={calculateMargins(xAxis, yAxis, undefined)}
        >
          {calculateXAxis(xAxis, {
            interval: 0,
            tick: { fontSize: 12 },
            dataKey: 'name',
          })}

          <CartesianGrid />

          {calculateYAxis(yAxis, { type: 'number' })}

          <Tooltip />
          <Legend />

          {Array.from(keysForChart).map(name => (
            <Bar
              key={name}
              {...populateDefaultChartProps(name, labels[name])}
              stackId={stacked ? 'a' : undefined}
            />
          ))}
        </BarChart>
      </ResponsiveContainer>
    );
  }
}
