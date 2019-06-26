import ChartFunctions, {
  ChartData,
  ChartDefinition,
  ChartProps,
  DataSetResult,
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
    const { data, height, xAxis, yAxis, width, meta, dataSets } = this.props;

    if (dataSets === undefined) return <div />;

    const chartData: ChartData[] = ChartFunctions.generateDataGroupedByGroups(
      ChartFunctions.filterResultsByDataSet(dataSets, data.result),
      meta,
    );

    const chartKeys: Set<string> = chartData.reduce(
      (keys: Set<string>, next) => {
        return new Set([
          ...Array.from(keys),
          ...(next.data || []).map(({ name }) => name),
        ]);
      },
      new Set<string>(),
    );

    return (
      <ResponsiveContainer width={width || 900} height={height || 300}>
        <BarChart
          data={chartData}
          margin={ChartFunctions.calculateMargins(xAxis, yAxis, undefined)}
        >
          {ChartFunctions.calculateXAxis(xAxis, {
            interval: 0,
            tick: { fontSize: 12 },
            dataKey: 'name',
          })}

          <CartesianGrid />

          {ChartFunctions.calculateYAxis(yAxis, { type: 'number' })}

          <Tooltip />
          <Legend />

          {Array.from(chartKeys).map((dataKey, index) => (
            <Bar
              key={dataKey}
              dataKey={dataKey}
              fill={colours[index]}
              name={meta.indicators[dataKey].label || 'a'}
              unit={meta.indicators[dataKey].unit || 'a'}
            />
          ))}
        </BarChart>
      </ResponsiveContainer>
    );
  }
}
