import ChartFunctions, {
  ChartDefinition,
  ChartProps,
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
    const { data, indicators, height, xAxis, yAxis, width, meta } = this.props;
    const chartData = data.result.map(({ measures, year, timeIdentifier }) => ({
      name: `${meta.timePeriods[`${year}_${timeIdentifier}`].label}`,
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
            dataKey: (xAxis.key || ['name'])[0],
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
}
