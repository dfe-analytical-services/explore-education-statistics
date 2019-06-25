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
      meta,
      height,
      width,
      xAxis,
      referenceLines,
      yAxis,
      stacked,
      indicators,
    } = this.props;

    const chartData = data.result.map(({ measures, year, timeIdentifier }) => ({
      name: `${meta.timePeriods[`${year}_${timeIdentifier}`].label}`,
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
            dataKey: (xAxis.key || ['name'])[0],
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
}
