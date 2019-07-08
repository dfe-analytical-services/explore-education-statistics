import {
  calculateMargins,
  calculateXAxis,
  calculateYAxis,
  ChartDataB,
  ChartDefinition,
  ChartProps,
  createDataForAxis,
  getKeysForChart,
  mapNameToNameLabel,
  colours,
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

export default class VerticalBarBlock extends Component<ChartProps> {
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
      },
    ],
  };

  public render() {
    const { data, height, width, labels, axes } = this.props;

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

          {Array.from(keysForChart).map((dataKey, index) => (
            <Bar
              key={dataKey}
              dataKey={dataKey}
              fill={colours[index]}
              name={labels[dataKey] && labels[dataKey].label}
              unit={labels[dataKey] && labels[dataKey].unit}
            />
          ))}
        </BarChart>
      </ResponsiveContainer>
    );
  }
}
